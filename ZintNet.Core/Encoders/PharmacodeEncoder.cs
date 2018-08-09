/* PharmacodeEncoder.cs - Handles Pharamacode and Pharmacode 2 Track symbols */

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
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class PharmacodeEncoder : SymbolEncoder
    {
        public PharmacodeEncoder(Symbology symbolId, string barcodeMessage)
        {
            this.symbolId = symbolId;
            this.barcodeMessage = barcodeMessage;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.Pharmacode:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    PharmaOne();
                    //barcodeText = new String(barcodeData);
                    break;

                case Symbology.Pharmacode2Track:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    PharmaTwo();
                    //barcodeText = new String(barcodeData);
                    break;
            }

            return Symbol;
        }

        private void PharmaOne()
        {
            ulong tester;
            List<char> intermediate = new List<char>(); /* 131070 -> 17 bits */
            StringBuilder rowPattern = new StringBuilder(); /* 17 * 2 + 1 */
            int inputLength = barcodeData.Length;

            if (inputLength > 6)
                throw new InvalidDataLengthException("Pharmacode: Input data too long.");

            tester = ulong.Parse(barcodeMessage, CultureInfo.CurrentCulture);
            if ((tester < 3) || (tester > 131070))
                throw new InvalidDataException("Pharmacode: Input data out of range.");

            do {
                if ((tester & 1) == 0)
                {
                    intermediate.Add('W');
                    tester = (tester - 2) / 2;
                }

                else
                {
                    intermediate.Add('N');
                    tester = (tester - 1) / 2;
                }

            } while (tester != 0);

            int iLength = intermediate.Count;
            for (int c = iLength - 1; c >= 0; c--)
            {
                if (intermediate[c] == 'W') 
                    rowPattern.Append("32");

                else
                    rowPattern.Append("12");
            }

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void PharmaTwo()
        {
            char[] symbolPattern;
            int patternLength;
            int index = 0;
            int length = barcodeData.Length;
            SymbolData symbolData;
            byte[] rowData1;
            byte[] rowData2;

            if (length > 8)
                throw new InvalidDataLengthException("Pharmacode 2-Track: Input data too long.");

            symbolPattern = PharmaTwoCalculator();
            patternLength = symbolPattern.Length;
            rowData1 = new byte[patternLength * 2];
            rowData2 = new byte[patternLength * 2];

            for (int l = 0; l < patternLength; l++)
            {
                if ((symbolPattern[l] == '2') || (symbolPattern[l] == '3'))
                    rowData1[index] = 1;

                if ((symbolPattern[l] == '1') || (symbolPattern[l] == '3'))
                    rowData2[index] = 1;

                index += 2;
            }

            symbolData = new SymbolData(rowData1);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2);
            Symbol.Add(symbolData);
        }

        private char[] PharmaTwoCalculator()
        {
            /* This code uses the Two Track Pharamacode defined in the document at
               http://www.laetus.com/laetus.php?request=file&id=69 and using a modified
               algorithm from the One Track system. This standard accepts integer values
               from 4 to 64570080. */

            char[] pattern;
            ulong tester;
            List<char> intermediate = new List<char>();

            tester = ulong.Parse(barcodeMessage, CultureInfo.CurrentCulture);
            if ((tester < 4) || (tester > 64570080))
                throw new InvalidDataException("Pharmacode 2-Track: Input data out of range.");

            do {
                switch (tester % 3)
                {
                    case 0:
                        intermediate.Add('3');
                        tester = (tester - 3) / 3;
                        break;

                    case 1:
                        intermediate.Add('1');
                        tester = (tester - 1) / 3;
                        break;

                    case 2:
                        intermediate.Add('2');
                        tester = (tester - 2) / 3;
                        break;
                }

            } while (tester != 0);

            int iLength = intermediate.Count;
            pattern = new char[iLength];

            for (int c = iLength - 1; c >= 0; c--)
                pattern[iLength - c - 1] = intermediate[c];

            return pattern;
        }
    }
}
