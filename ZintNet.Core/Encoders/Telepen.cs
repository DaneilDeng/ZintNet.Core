/* TelepenEncoder.cs - Handles Telepen and Telepen Numeric 1D symbols */

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
using System.Collections.ObjectModel;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class TelepenEncoder : SymbolEncoder
    {
        #region Tables
        private static string[] TeleTable = {
            "1111111111111111", "1131313111", "33313111", "1111313131", "3111313111", "11333131", "13133131", "111111313111",
            "31333111", "1131113131", "33113131", "1111333111", "3111113131", "1113133111", "1311133111", "111111113131",
            "3131113111", "11313331", "333331", "111131113111", "31113331", "1133113111", "1313113111", "1111113331",
            "31131331", "113111113111", "3311113111", "1111131331", "311111113111", "1113111331", "1311111331", "11111111113111",
            "31313311", "1131311131", "33311131", "1111313311", "3111311131", "11333311", "13133311", "111111311131",
            "31331131", "1131113311", "33113311", "1111331131", "3111113311", "1113131131", "1311131131", "111111113311",
            "3131111131", "1131131311", "33131311", "111131111131", "3111131311", "1133111131", "1313111131", "111111131311",
            "3113111311", "113111111131", "3311111131", "111113111311", "311111111131", "111311111311", "131111111311", "11111111111131",
            "3131311111", "11313133", "333133", "111131311111", "31113133", "1133311111", "1313311111", "1111113133",
            "313333", "113111311111", "3311311111", "11113333", "311111311111", "11131333", "13111333", "11111111311111",
            "31311133", "1131331111", "33331111", "	1111311133", "3111331111", "11331133", "13131133", "111111331111",
            "3113131111", "1131111133", "33111133", "111113131111", "3111111133", "111311131111", "131111131111", "111111111133",
            "31311313", "113131111111", "3331111111", "1111311313", "311131111111", "11331313", "13131313", "11111131111111",
            "3133111111", "1131111313", "33111313", "111133111111", "3111111313", "111313111111", "131113111111", "111111111313",
            "313111111111", "1131131113", "33131113", "11113111111111", "3111131113", "113311111111", "131311111111", "111111131113",
            "3113111113", "11311111111111", "331111111111", "111113111113", "31111111111111", "111311111113", "131111111113" };
        #endregion

        public TelepenEncoder(Symbology symbology, string barcodeMessage)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.Telepen:
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    Telepen();
                    barcodeText = new String(barcodeData);
                    break;

                case Symbology.TelepenNumeric:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    TelepenNumeric();
                    barcodeText = new String(barcodeData);
                    break;
            }

            return Symbol;
        }

        private void Telepen()
        {
            int check_digit;
            int count = 0;
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 30)
                throw new InvalidDataLengthException("Telepen: Input data too long.");

            // Start character.
            rowPattern.Append(TeleTable['_']);

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] > 126)
                    throw new InvalidDataException("Telpen: Invalid characters in input data.");   // Cannot encode extended ASCII.

                rowPattern.Append(TeleTable[barcodeData[i]]);
                count += barcodeData[i];
            }

            check_digit = 127 - (count % 127);
            if (check_digit == 127)
                check_digit = 0;

            rowPattern.Append(TeleTable[check_digit]);
            // Stop character.
            rowPattern.Append(TeleTable['z']);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void TelepenNumeric()
        {
            int checkDigit, glyph;
            int count = 0;
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            barcodeData = ArrayExtensions.ToUpper(barcodeData);

            if (inputLength > 60)
                throw new InvalidDataLengthException("Telepen Numeric: Input data too long.");

            for (int i = 0; i < inputLength; i++)
            {
                if (CharacterSets.TelepenNumeric.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Telepen Numeric: Invalid characters in input data.");
            }

            // Length must even, add a leading zero if required.
            if ((inputLength & 1) == 1)
            {
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, '0');
                inputLength = barcodeData.Length;
            }

            // Start character.
            rowPattern.Append(TeleTable['_']);

            for (int i = 0; i < inputLength; i += 2)
            {
                if (barcodeData[i] == 'X')
                    throw new InvalidDataFormatException("Telepen Numeric: Invalid position of X in Telepen data.");

                if (barcodeData[i + 1] == 'X')
                {
                    glyph = (barcodeData[i] - '0') + 17;
                    count += glyph;
                }

                else
                {
                    glyph = ((10 * (barcodeData[i] - '0')) + (barcodeData[i + 1] - '0'));
                    glyph += 27;
                    count += glyph;
                }

                rowPattern.Append(TeleTable[glyph]);
            }

            checkDigit = 127 - (count % 127);
            if (checkDigit == 127)
                checkDigit = 0;

            rowPattern.Append(TeleTable[checkDigit]);
            // Stop character.
            rowPattern.Append(TeleTable['z']);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}
