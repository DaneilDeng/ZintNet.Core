/* CodabarEncoder.cs Handles Codabar 1D symbol */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Robin Stuart and other Zint Authors and Contributors.
  
    libzint - the open source barcode library
    Copyright (C) 2009-2016 Robin Stuart <rstuart114@gmail.com>

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
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class CodabarEncoder : SymbolEncoder
    {
        #region Tables

        private static string[] CodabarTable = {
                "11111221", "11112211", "11121121", "22111111", "11211211", "21111211",
                "12111121", "12112111", "12211111", "21121111", "11122111", "11221111", "21112121", "21211121",
                "21212111", "11212121", "11221211", "12121121", "11121221", "11122211"};
        #endregion

        public CodabarEncoder(string barcodeMessage)
        {
            this.barcodeMessage = barcodeMessage;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
            Codabar();
            return Symbol;
        }

        private void Codabar()
        {
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            for (int i = 0; i < inputLength; i++)
            {
                if (CharacterSets.CodaBarSet.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Codabar: Invalid characters in input data.");
            }

            // Codabar must begin and end with the characters A, B, C or D
            if ((barcodeData[0] != 'A') && (barcodeData[0] != 'B') && (barcodeData[0] != 'C') && (barcodeData[0] != 'D'))
                throw new InvalidDataException("Codabar: Invalid start character in input data.");

            if ((barcodeData[inputLength - 1] != 'A') && (barcodeData[inputLength - 1] != 'B') &&
               (barcodeData[inputLength - 1] != 'C') && (barcodeData[inputLength - 1] != 'D'))
                throw new InvalidDataException("Codabar: Invalid stop character in input data.");

            for (int i = 0; i < inputLength; i++)
            {
                int iValue = CharacterSets.CodaBarSet.IndexOf(barcodeData[i]);
                rowPattern.Append(CodabarTable[iValue]);
            }
           
            barcodeText = new String(barcodeData);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}