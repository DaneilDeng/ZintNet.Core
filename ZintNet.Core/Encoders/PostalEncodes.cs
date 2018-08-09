/* PostalEncoder.cs - Handles PostNet, PLANET, Korean, Japan Post, FIM, RM4SCC and Flattermarken */

/*  ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Robin Stuart and other Zint Authors and Contributors.
  
    libzint - the open source barcode library
    Copyright (C) 2008-2016 Robin Stuart <rstuart114@gmail.com>
    Including bug fixes by Bryan Hatton

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
using System.Globalization;
using System.Text;


namespace ZintNet.Core.Encoders
{
    internal class PostalEncoder : SymbolEncoder
    {
        #region Tables
        private static string[] PostNetTable = {
            "LLSSS", "SSSLL", "SSLSL", "SSLLS", "SLSSL", "SLSLS", "SLLSS", "LSSSL",
            "LSSLS", "LSLSS" };

        private static string[] PlanetTable = {
            "SSLLL", "LLLSS", "LLSLS", "LLSSL", "LSLLS", "LSLSL", "LSSLL", "SLLLS",
            "SLLSL", "SLSLL" };

        private static string[] KoreanTable = {
            "1313150613", "0713131313", "0417131313", "1506131313",
            "0413171313", "17171313", "1315061313", "0413131713", "17131713", "13171713" };

        private static string[] JapanTable = {
            "114", "132", "312", "123", "141", "321", "213", "231", "411", "144",
            "414", "324", "342", "234", "432", "243", "423", "441", "111" };

        private static string[] RoyalValues = {
            "11", "12", "13", "14", "15", "10", "21", "22", "23", "24", "25",
            "20", "31", "32", "33", "34", "35", "30", "41", "42", "43", "44", "45", "40", "51", "52",
            "53", "54", "55", "50", "01", "02", "03", "04", "05", "00" };

        /* 0 = Full, 1 = Ascender, 2 = Descender, 3 = Tracker */
        private static string[] RoyalTable = {
            "3300", "3210", "3201", "2310", "2301", "2211", "3120", "3030", "3021",
            "2130", "2121", "2031", "3102", "3012", "3003", "2112", "2103", "2013", "1320", "1230",
            "1221", "0330", "0321", "0231", "1302", "1212", "1203", "0312", "0303", "0213", "1122",
            "1032", "1023", "0132", "0123", "0033" };

        private static string[] FlatTable = { "0504", "18", "0117", "0216", "0315", "0414", "0513", "0612", "0711", "0810" };
        #endregion

        public PostalEncoder(Symbology symbolId, string barcodeMessage)
        {
            this.symbolId = symbolId;
            this.barcodeMessage = barcodeMessage;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.PostNet:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    PostNet();
                    break;

                case Symbology.Planet:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    Planet();
                    break;

                case Symbology.KoreaPost:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    KoreaPost();
                    break;

                case Symbology.FIM:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    FIM();
                    break;

                case Symbology.RoyalMail:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    RoyalMail();
                    break;

                case Symbology.KixCode:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    KixCode();
                    break;

                case Symbology.DaftCode:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    DaftCode();
                    break;

                case Symbology.Flattermarken:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    Flattermarken();
                    break;

                case Symbology.JapanPost:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    JapanPost();
                    break;
            }

            return Symbol;
        }

        private void PostNet()
        {
            int sum = 0;
            int checkValue;
            StringBuilder symbolPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength != 5 && inputLength != 9 && inputLength != 11)
                throw new InvalidDataLengthException("PostNet Code: Invalid input data length.");

            symbolPattern.Append('L');
            for (int i = 0; i < inputLength; i++)
            {
                int value = barcodeData[i] - '0';
                symbolPattern.Append(PostNetTable[value]);
                sum += value;
            }

            checkValue = (10 - (sum % 10)) % 10;
            symbolPattern.Append(PostNetTable[checkValue]);
            symbolPattern.Append('L');
            USPostWriter(symbolPattern);
            barcodeText = new string(barcodeData);
            checkDigitText = new string((char)(checkValue + '0'), 1);
        }

        private void Planet()
        {
            int sum = 0;
            int checkValue;
            StringBuilder symbolPattern = new StringBuilder();

            int inputLength = barcodeData.Length;
            if (inputLength != 11 && inputLength != 13)
                throw new InvalidDataLengthException("Planet Code: Invalid input data length.");

            symbolPattern.Append('L');
            for (int i = 0; i < inputLength; i++)
            {
                int value = barcodeData[i] - '0';
                symbolPattern.Append(PlanetTable[value]);
                sum += value;
            }

            checkValue = (10 - (sum % 10)) % 10;
            symbolPattern.Append(PlanetTable[checkValue]);
            symbolPattern.Append('L');
            USPostWriter(symbolPattern);
            barcodeText = new string(barcodeData);
            checkDigitText = new string((char)(checkValue + '0'), 1);
        }

        private void KoreaPost()
        {
            int sum = 0;
            int checkValue;
            int value;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 6)
                throw new InvalidDataLengthException("Korean Post: Input data too long.");

            if (inputLength < 6)
            {
                string zeros = new String('0', 6 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
                inputLength = barcodeData.Length;
            }

            sum = 0;
            for (int l = 0; l < 6; l++)
                sum += barcodeData[l] - '0';

            checkValue = (10 - (sum % 10)) % 10;
            barcodeData = ArrayExtensions.Insert(barcodeData, barcodeData.Length, checkValue.ToString(CultureInfo.CurrentCulture));
            for (int l = 5; l >= 0; l--)
            {
                value = barcodeData[l] - '0';
                rowPattern.Append(KoreanTable[value]);
            }

            value = barcodeData[6] - '0';
            rowPattern.Append(KoreanTable[value]);
            barcodeText = new string(barcodeData);

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void FIM()
        {
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 1)
                throw new InvalidDataLengthException("FIM Code: Input data too long.");

            switch ((char)barcodeData[0])
            {
                case 'a':
                case 'A':
                    rowPattern.Append("111515111");
                    break;

                case 'b':
                case 'B':
                    rowPattern.Append("13111311131");
                    break;

                case 'c':
                case 'C':
                    rowPattern.Append("11131313111");
                    break;

                case 'd':
                case 'D':
                    rowPattern.Append("1111131311111");
                    break;

                default:
                    throw new InvalidDataException("FIM Code: Invalid characters in input data.");
            }

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void RoyalMail()
        {
            int top = 0;
            int bottom = 0;
            string values;
            int row;
            int column;
            int checkValue;
            StringBuilder symbolPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 50)
                throw new InvalidDataLengthException("Royal Mail: Input data too long.");

            for (int i = 0; i < inputLength; i++)
            {
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);
                if (CharacterSets.KRSET.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Royal Mail: Invalid characters in input data.");
            }

            symbolPattern.Append('1');  // Start character.
            for (int i = 0; i < inputLength; i++)
            {
                int position = CharacterSets.KRSET.IndexOf(barcodeData[i]); 
                symbolPattern.Append(RoyalTable[position]);
                values = RoyalValues[position];
                top += values[0] - '0';
                bottom += values[1] - '0';
            }

            // Calculate the check digit.
            row = (top % 6) - 1;
            column = (bottom % 6) - 1;
            if (row == -1)
                row = 5;

            if (column == -1)
                column = 5;

            checkValue = (6 * row) + column;
            symbolPattern.Append(RoyalTable[checkValue]);
            symbolPattern.Append('0');  // Stop character.
            BuildSymbol(symbolPattern);
            barcodeText = new string(barcodeData);
        }

        private void KixCode()
        {
            StringBuilder symbolPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 18)
                throw new InvalidDataLengthException("Dutch Post: Input data too long.");

            for (int i = 0; i < inputLength; i++)
            {
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);
                if (CharacterSets.KRSET.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Dutch Post: Invalid characters in input data.");
            }

            for (int i = 0; i < inputLength; i++)
            {
                int position = CharacterSets.KRSET.IndexOf(barcodeData[i]); // RM & KIX character set is the same as Code32.
                symbolPattern.Append(RoyalTable[position]);
            }

            BuildSymbol(symbolPattern);
            barcodeText = new string(barcodeData);
        }

        private void DaftCode()
        {
            StringBuilder symbolPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 50)
                throw new InvalidDataLengthException("DAFT Code: Input data too long.");

            for (int i = 0; i < inputLength; i++)
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] == 'D')
                    symbolPattern.Append("2");

                if (barcodeData[i] == 'A')
                    symbolPattern.Append("1");

                if (barcodeData[i] == 'F')
                    symbolPattern.Append("0");

                if (barcodeData[i] == 'T')
                    symbolPattern.Append("3");
            }

            BuildSymbol(symbolPattern);
        }

        private void Flattermarken()
        {
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 90)
                throw new InvalidDataLengthException("Flattermarken: Input data too long.");

            for (int l = 0; l < inputLength; l++)
            {
                int value = barcodeData[l] - '0';
                rowPattern.Append(FlatTable[value]);
            }

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void JapanPost()
        {
            int sourceIndex = 0;
            int intermediateIndex = 0;
            int rowIndex = 0;
            int sum, checkValue;
            char checkDigit = '\0';
            int patternLength;
            SymbolData symbolData;
            byte[] rowData1;
            byte[] rowData2;
            byte[] rowData3;
            StringBuilder intermediate = new StringBuilder();
            StringBuilder symbolPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 20)
                throw new InvalidDataLengthException("Flattermarken: Input data too long.");
    
            //inter_posn = 0;

            for (int i = 0; i < inputLength; i++)
            {
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);
                if (CharacterSets.SHKASUTSET.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Japan Post: Invalid characters in input data.");
            }

            intermediate.Append('d', 20);  // Pad character CC4.

            //index = 0;
            //intermediateIndex = 0;
            do {
                if (((barcodeData[sourceIndex] >= '0') && (barcodeData[sourceIndex] <= '9')) || (barcodeData[sourceIndex] == '-'))
                {
                    intermediate[intermediateIndex] = barcodeData[sourceIndex];
                    intermediateIndex++;
                }
                
                else
                {
                    if ((barcodeData[sourceIndex] >= 'A') && (barcodeData[sourceIndex] <= 'J'))
                    {
                        intermediate[intermediateIndex] = 'a';
                        intermediate[intermediateIndex + 1] = (char)(barcodeData[sourceIndex] - 'A' + '0');
                        intermediateIndex += 2;
                    }

                    if ((barcodeData[sourceIndex] >= 'K') && (barcodeData[sourceIndex] <= 'T'))
                    {
                        intermediate[intermediateIndex] = 'b';
                        intermediate[intermediateIndex + 1] = (char)(barcodeData[sourceIndex] - 'K' + '0');
                        intermediateIndex += 2;
                    }
                    if ((barcodeData[sourceIndex] >= 'U') && (barcodeData[sourceIndex] <= 'Z'))
                    {
                        intermediate[intermediateIndex] = 'c';
                        intermediate[intermediateIndex + 1] = (char)(barcodeData[sourceIndex] - 'U' + '0');
                        intermediateIndex += 2;
                    }
                }

                sourceIndex++;
            } while ((sourceIndex < inputLength) && (intermediateIndex < 20));

            symbolPattern.Append("13"); // Start.

            sum = 0;
            for (int i = 0; i < 20; i++)
            {
                int position = CharacterSets.KASUTSET.IndexOf(intermediate[i]);
                symbolPattern.Append(JapanTable[position]);
                sum += CharacterSets.CHKASUTSET.IndexOf(intermediate[i]);
            }

            // Calculate check digit.
            checkValue = 19 - (sum % 19);
            if (checkValue == 19)
                checkValue = 0;

            if (checkValue <= 9)
                checkDigit = (char)(checkValue + '0');

            if (checkValue == 10)
                checkDigit = '-';

            if (checkValue >= 11)
                checkDigit = (char)((checkValue - 11) + 'a');

            symbolPattern.Append(JapanTable[CharacterSets.KASUTSET.IndexOf(checkDigit)]);
            symbolPattern.Append("31"); // Stop.

            patternLength = symbolPattern.Length;
            rowData1 = new byte[patternLength * 2];
            rowData2 = new byte[patternLength * 2];
            rowData3 = new byte[patternLength * 2];
            
            for (int l = 0; l < patternLength; l++)
            {
                if ((symbolPattern[l] == '2') || (symbolPattern[l] == '1'))
                    rowData1[rowIndex] = 1;

                rowData2[rowIndex] = 1;
                if ((symbolPattern[l] == '3') || (symbolPattern[l] == '1'))
                    rowData3[rowIndex] = 1;

                rowIndex += 2;
            }

            symbolData = new SymbolData(rowData1, 3.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2, 2.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData3, 3.0f);
            Symbol.Add(symbolData);
        }

        private void USPostWriter(StringBuilder symbolPattern)
        {
            int patternLength;
            int index = 0;
            SymbolData symbolData;
            byte[] rowData1;
            byte[] rowData2;

            patternLength = symbolPattern.Length;
            rowData1 = new byte[patternLength * 3];
            rowData2 = new byte[patternLength * 3];

            for (int p = 0; p < patternLength; p++)
            {
                if (symbolPattern[p] == 'L')
                    rowData1[index] = 1;

                rowData2[index] = 1;
                index += 3;
            }

            symbolData = new SymbolData(rowData1, 6.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2, 6.0f);
            Symbol.Add(symbolData);
        }

        private void BuildSymbol(StringBuilder barPattern)
        {
            int patternLength;
            int index = 0;
            SymbolData symbolData;
            byte[] rowData1;
            byte[] rowData2;
            byte[] rowData3;

            patternLength = barPattern.Length;
            rowData1 = new byte[patternLength * 2];
            rowData2 = new byte[patternLength * 2];
            rowData3 = new byte[patternLength * 2];

            for (int p = 0; p < patternLength; p++)
            {
                if ((barPattern[p] == '1') || (barPattern[p] == '0'))
                    rowData1[index] = 1;

                rowData2[index] = 1;
                if ((barPattern[p] == '2') || (barPattern[p] == '0'))
                    rowData3[index] = 1;

                index += 2;
            }

            symbolData = new SymbolData(rowData1, 3.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2, 2.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData3, 3.0f);
            Symbol.Add(symbolData);
        }
    }
}
