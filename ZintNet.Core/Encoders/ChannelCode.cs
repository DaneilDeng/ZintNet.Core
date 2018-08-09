/* ChannelCodeEncoder.cs - Handles Channel Code 1D symbols */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Robin Stuart and other Zint Authors and Contributors.
  
    libzint - the open source barcode library
    Copyright (C) 2008-2016 Robin Stuart <rstuart114@gmail.com>

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions
    are met:

    1. Redistributions of source code must retain the above copyright 
       notice, this list of conditions and the following disclaimer.  
    2. Redistributions in binary form must reproduce the above copyright
       notice, this list of conditions and the following disclaimer in the
       documentation and/or other materials provided with the distribution.  
    3. Neither the name of the project nor the names of its contributors
       may be used to endorse or promote products derived from this software
       without specific prior written permission. 

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
    ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
    LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
    OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
    SUCH DAMAGE.
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class ChannelCodeEncoder : SymbolEncoder
    {
        private int channels = 0;
        
        // Global Variables for Channel Code.
        private StringBuilder rowPattern;
        int[] S;
        int[] B;
        long index;
        long targetValue;
        

        public ChannelCodeEncoder(Symbology symbology, string barcodeMessage, int channels)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.channels = channels;

        }

        public override Collection<SymbolData> EncodeData()
        {
            this.Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
            ChannelCode();
            return Symbol;
        }

        private void ChannelCode()
        {
            bool outOfRange = false;
            targetValue = 0;
            rowPattern = new StringBuilder();
            S = new int[11];
            B = new int[11];
            int inputLength = barcodeData.Length;

            if (inputLength > 7)
                throw new InvalidDataLengthException("Channel Code: Input data too long.");

            if (channels == 0)
                channels = inputLength + 1;

            if (channels == 2)
                channels = 3;

            for (int i = 0; i < inputLength; i++)
            {
                targetValue *= 10;
                targetValue += barcodeData[i] - '0';
            }

            switch (channels)
            {
                case 3:
                    if (targetValue > 26)
                        outOfRange = true;
                    break;

                case 4:
                    if (targetValue > 292)
                        outOfRange = true;
                    break;

                case 5:
                    if (targetValue > 3493)
                        outOfRange = true;
                    break;

                case 6:
                    if (targetValue > 44072)
                        outOfRange = true;
                    break;

                case 7:
                    if (targetValue > 576688)
                        outOfRange = true;
                    break;

                case 8:
                    if (targetValue > 7742862)
                        outOfRange = true;
                    break;
            }

            if (outOfRange)
                throw new InvalidDataException("Channel Code: Input data out of range.");

            B[0] = S[1] = B[1] = S[2] = B[2] = 1;
            index = 0;
            NextS(channels, 3, channels, channels);
            string zeros = new String('0', channels - 1 - inputLength);

            barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
            inputLength = barcodeData.Length;

            barcodeText = new string(barcodeData);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        /* NextS() and NextB() are from ANSI/AIM BC12-1998 and are Copyright (c) AIM 1997.
           They are used here on the understanding that they form part of the specification
           for Channel Code and therefore their use is permitted under the following terms
           set out in that document:

           "It is the intent and understanding of AIM, that the symbology presented in this
           specification is entirely in the public domain and free of all use restrictions,
           licenses and fees. AIM USA, its member companies, or individual officers
           assume no liability for the use of this document." */

        private void NextS(int channel, int i, int maxS, int maxB)
        {
            int s;

            for (s = (i < channel + 2) ? 1 : maxS; s <= maxS; s++)
            {
                S[i] = s;
                NextB(channel, i, maxB, maxS + 1 - s);
            }
        }

        private void NextB(int channel, int i, int maxB, int maxS)
        {
            int b;

            b = (S[i] + B[i - 1] + S[i - 1] + B[i - 2] > 4) ? 1 : 2;
            if (i < channel + 2)
            {
                for (; b <= maxB; b++)
                {
                    B[i] = b;
                    NextS(channel, i + 1, maxS, maxB + 1 - b);
                }
            }

            else if (b <= maxB)
            {
                B[i] = maxB;
                CheckCharacter();
                index++;
            }
        }

        private void CheckCharacter()
        {
            if (index == targetValue)
            {
                // Target reached - save the generated pattern.
                rowPattern.Append("11110");
                for (int i = 0; i < 11; i++)
                {
                    rowPattern.Append((char)(S[i] + '0'));
                    rowPattern.Append((char)(B[i] + '0'));
                }
            }
        }
    }
}
