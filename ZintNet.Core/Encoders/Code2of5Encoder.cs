/* Code2of5Encoder.cs - Handles Code 2 of 5 based 1D symbols */

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
    // Code 2of5 Family symbol encoder.
    internal class Code2of5Encoder : SymbolEncoder
    {
        # region Tables
        // Standard (Industrial) 2 of 5 bar pattens.
        static string[] Standard2of5Table =	{
            "1111313111", "3111111131", "1131111131", "3131111111", "1111311131",
            "3111311111", "1131311111", "1111113131", "3111113111", "1131113111" };

        // Interleaved bar pattens.
        static string[] Interleaved2of5Table = {
             "11221", "21112", "12112", "22111", "11212",
             "21211", "12211", "11122", "21121", "12121" };

        // Matrix bar patterns.
        static string[] Matrix2of5Table = {
            "113311", "311131", "131131", "331111", "113131",
            "313111", "133111", "111331", "311311", "131311" };
        # endregion

        I2of5CheckDigitType optCheckDigitType;
        private bool optCheckDigit;
        private int deutschePostSize = 0;

        public Code2of5Encoder(Symbology symbology, string barcodeMessage, bool optCheckDigit, I2of5CheckDigitType optCheckDigitType)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optCheckDigit = optCheckDigit;
            this.optCheckDigitType = optCheckDigitType;
            this.Symbol = new Collection<SymbolData>();
            this.checkDigitText = string.Empty;
            this.barcodeText = string.Empty;
        }

        public override Collection<SymbolData> EncodeData()
        {
            barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
            switch (symbolId)
            {
                case Symbology.Standard2of5:
                    Standard2of5();
                    break;

                case Symbology.Matrix2of5:
                    Matrix2of5();
                    break;

                case Symbology.IATA2of5:
                    IATA2of5();
                    break;

                case Symbology.Interleaved2of5:
                    Interleaved2of5();
                    break;

                case Symbology.DataLogic2of5:
                    DataLogic2of5();
                    break;

                case Symbology.ITF14:
                    optCheckDigit = false;
                    ITF14();
                    break;

                case Symbology.DeutschePostIdentCode:
                    optCheckDigit = false;
                    deutschePostSize = 11;
                    DeutschePost();
                    break;

                case Symbology.DeutshePostLeitCode:
                    optCheckDigit = false;
                    deutschePostSize = 13;
                    DeutschePost();
                    break;
            }

            return Symbol;
        }

        private void Standard2of5()
        {
            int index;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 45)
                throw new InvalidDataLengthException("Code 2of5(Standard): Input data too long.");

            // Start character.
            rowPattern.Append("313111");
            for (int i = 0; i < inputLength; i++)
            {
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                rowPattern.Append(Standard2of5Table[index]);
            }

            // Stop character.
            rowPattern.Append("31113");
            // Set the human readable text.
            barcodeText = new string(barcodeData);

            // Expand row into the symbol data..
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void Matrix2of5()
        {
            int index;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 80)
                throw new InvalidDataLengthException("Code 2of5(Matrix): Input data too long.");

            // Add start character.
            rowPattern.Append("411111");
            for (int i = 0; i < inputLength; i++)
            {
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                rowPattern.Append(Matrix2of5Table[index]);
            }

            // Add stop character.
            rowPattern.Append("41111");

            // Set the human readable text.
            barcodeText = new string(barcodeData);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void IATA2of5()
        {
            int index;
            int mLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (mLength > 45)
                throw new InvalidDataLengthException("Code 2of5(IATA): Input data too long.");

            // Add start character.
            rowPattern.Append("1111");
            for (int i = 0; i < mLength; i++)
            {
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                rowPattern.Append(Standard2of5Table[index]);
            }

            // Add stop character.
            rowPattern.Append("311");

            // Set the human readable text.
            barcodeText = new string(barcodeData);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        /// <summary>
        /// Code 2of5 Data Logic.
        /// </summary>
        private void DataLogic2of5()
        {
            int index;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 80)
                throw new InvalidDataLengthException("Code 2of5(Data Logic): Input data too long.");

            // Start character.
            rowPattern.Append("1111");
            for (int i = 0; i < inputLength; i++)
            {
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                rowPattern.Append(Matrix2of5Table[index]);
            }

            // Stop character.
            rowPattern.Append("311");

            // Set the human readable text.
            barcodeText = new string(barcodeData);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        /// <summary>
        /// Encodes ITF14
        /// </summary>
        private void ITF14()
        {
            char checkDigit;
            int inputLength = barcodeData.Length;

            if (inputLength < 13)
            {
                string zeros = new String('0', 13 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
                inputLength = barcodeData.Length;
            }

            if (inputLength == 14)
            {
                char cd = barcodeData[inputLength - 1];
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData, inputLength - 1);
                if (checkDigit != cd)
                    throw new InvalidDataException("Code ITF-14: Invalid check digit.");
            }

            else if (inputLength == 13)
            {
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData, inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
                inputLength = barcodeData.Length;
            }

            else
                throw new InvalidDataLengthException("Code ITF-14: Input data too long.");

            Interleaved2of5();

            // Set the human readable text.
            barcodeText = string.Empty;
            for (int i = 0; i < inputLength; i++)
            {
                barcodeText += barcodeData[i];
                // Insert spaces at these points.
                if (i == 0 || i == 2 || i == 7 || i == 12)
                    barcodeText += "  ";
            }
        }

        /// <summary>
        /// Encodes Interleaved 2 of 5.
        /// </summary>
        private void Interleaved2of5()
        {
            char checkDigit;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 89)
                throw new InvalidDataLengthException("Code 2of5 Interleaved: Input data too long.");

            if (optCheckDigit)
            {
                if (optCheckDigitType == I2of5CheckDigitType.USS)
                    checkDigit = CheckSum.Mod10CheckDigit(barcodeData);

                else
                    checkDigit = CheckSum.OPCCCheckDigit(barcodeData);

                barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
                inputLength = barcodeData.Length;
                checkDigitText += checkDigit;
            }

            // Interleaved 2of5 must have an even number of numeric characters.
            // Pad out with a leading "0" if not.
            if (inputLength % 2 != 0)
            {
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, '0');
                inputLength = barcodeData.Length;
            }

            rowPattern = new StringBuilder();
            string data1 = String.Empty;
            string data2 = String.Empty;
            int index;

            // Add the start character.
            rowPattern.Append("1111");
            for (int i = 0; i < inputLength - 1; i += 2)
            {
                // Get each character pair at a time
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                data1 = Interleaved2of5Table[index];
                index = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i + 1]);
                data2 = Interleaved2of5Table[index];

                // Interleave the data.
                for (int x = 0; x < 5; x++)
                {
                    rowPattern.Append(data1[x]);
                    rowPattern.Append(data2[x]);
                }
            }

            // Add stop character.
            rowPattern.Append("311");

            // Set the human readable text.
            if (optCheckDigit)
                barcodeText = new string(barcodeData, 0, barcodeData.Length - 1);   // Don't include the check digit.

            else
                barcodeText = new string(barcodeData);

            if(symbolId == Symbology.ITF14)
                rowPattern.Replace('2', '3'); // Use 3:1 bar ratio for ITF14

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void DeutschePost()
        {
            int count = 0;
            char checkDigit;
            int inputLength = barcodeData.Length;

            if (inputLength > deutschePostSize)
                throw new InvalidDataLengthException("Deutsche Post: Input data too long.");


            if (inputLength < deutschePostSize)
            {
                string zeros = new String('0', deutschePostSize - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
                inputLength = barcodeData.Length;
            }

            for (int i = inputLength - 1; i >= 0; i--)
            {
                count += 4 * (barcodeData[i] - '0');

                if ((i & 1) > 0)
                    count += 5 * (barcodeData[i] - '0');
            }

            checkDigit = (char)(((10 - (count % 10)) % 10) + '0');
            barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
            Interleaved2of5();
        }
    }
}