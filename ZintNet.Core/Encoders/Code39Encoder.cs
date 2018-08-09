/* Code39Encoder.cs - Handles Code 39 Based 1D symbols */

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class Code39Encoder : SymbolEncoder
    {
        #region Tables
        private static string[] Code39Table = {
            "1112212111", "2112111121", "1122111121", "2122111111", "1112211121",
		    "2112211111", "1122211111", "1112112121", "2112112111", "1122112111",
		    "2111121121", "1121121121", "2121121111", "1111221121", "2111221111",
		    "1121221111", "1111122121", "2111122111", "1121122111", "1111222111",
		    "2111111221", "1121111221", "2121111211", "1111211221", "2111211211",
		    "1121211211", "1111112221", "2111112211", "1121112211", "1111212211",
		    "2211111121", "1221111121", "2221111111", "1211211121", "2211211111",
		    "1221211111", "1211112121", "2211112111", "1221112111", "1212121111",
		    "1212111211", "1211121211", "1112121211", "1211212111"};

        private static string[] ExtendedC39Ctrl = {
            // Encoding the full ASCII character set in Code 39 (Table A2).
            "%U", "$A", "$B", "$C", "$D", "$E", "$F", "$G", "$H", "$I", "$J", "$K",
            "$L", "$M", "$N", "$O", "$P", "$Q", "$R", "$S", "$T", "$U", "$V", "$W", "$X", "$Y", "$Z",
            "%A", "%B", "%C", "%D", "%E", " ", "/A", "/B", "/C", "/D", "/E", "/F", "/G", "/H", "/I", "/J",
            "/K", "/L", "-", ".", "/O", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "/Z", "%F",
            "%G", "%H", "%I", "%J", "%V", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "%K", "%L", "%M", "%N", "%O",
            "%W", "+A", "+B", "+C", "+D", "+E", "+F", "+G", "+H", "+I", "+J", "+K", "+L", "+M", "+N", "+O",
            "+P", "+Q", "+R", "+S", "+T", "+U", "+V", "+W", "+X", "+Y", "+Z", "%P", "%Q", "%R", "%S", "%T" };
        #endregion

        private bool optionalCheckDigit;

        public Code39Encoder(Symbology symbolId, string barcodeMessage, bool optionalCheckDigit, EncodingMode mode)
        {
            this.symbolId = symbolId;
            this.barcodeMessage = barcodeMessage;
            this.optionalCheckDigit = optionalCheckDigit;
            this.encodingMode = mode;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.Code32:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    optionalCheckDigit = false;  // Code 32 generates it's own check digit.
                    Code32();
                    break;

                case Symbology.PharmaZentralNummer:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    optionalCheckDigit = false; // PharmaZentral generates it's own check digit.
                    PNZ();
                    break;

                case Symbology.Code39:
                    if (encodingMode == EncodingMode.HIBC)
                    {
                        barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                        optionalCheckDigit = true;  // HIBC requires a Code 39 check digit.
                        Code39();
                    }

                    else
                    {
                        barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                        Code39();
                    }

                    break;

                case Symbology.Code39Extended:
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    Code39Extended();
                    break;

                case Symbology.LOGMARS:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    Logmars();
                    break;

                case Symbology.VINCode:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    VINCode();
                    break;
            }

            return Symbol;
        }

        /// <summary>
        /// Encode Code 32 (Italian PhamaCode)
        /// </summary>
        private void Code32()
        {
            int checkValue = 0;
            int checkPart = 0;
            char checkDigit;
            char[] resultData = new char[6];
            int[] codeWord = new int[6];
            int inputLength = barcodeData.Length;

            if (inputLength != 8)
                throw new InvalidDataLengthException("Code 32: Requires 8 numeric characters.");

            // Calculate the check digit.
            for (int i = 0; i < 4; i++)
            {
                checkPart = (int)barcodeData[i * 2] - '0';
                checkValue += checkPart;
                checkPart = 2 * ((int)(barcodeData[(i * 2) + 1] - '0'));
                if (checkPart >= 10)
                    checkValue += (checkPart - 10) + 1;

                else
                    checkValue += checkPart;
            }

            checkDigit = (char)((checkValue % 10) + '0');
            barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);

            // Convert from decimal to base-32.
            int pharmacode = int.Parse(new string(barcodeData), CultureInfo.CurrentCulture);
            int devisor = 33554432;
            int remainder;
            for (int i = 5; i >= 0; i--)
            {
                codeWord[i] = pharmacode / devisor;
                remainder = pharmacode % devisor;
                pharmacode = remainder;
                devisor /= 32;
            }

            for (int i = 5; i >= 0; i--)
                resultData[5 - i] = CharacterSets.Code32Set[codeWord[i]];

            // Generate the barcode with Code 39 using the resultant data.
            Array.Copy(resultData, barcodeData, resultData.Length);
            Array.Resize(ref barcodeData, resultData.Length);
            Code39();
            barcodeText = "A" + barcodeMessage + checkDigit;
        }

        // Pharmazentral Nummer (PZN).
        private void PNZ()
        {
            int count = 0;
            int checkValue;
            int inputLength = barcodeData.Length;

            if (inputLength > 7)
                throw new InvalidDataLengthException("PNZ: input data too long.");

            barcodeData = ArrayExtensions.Insert(barcodeData, 0, '-');
            inputLength = barcodeData.Length;

            if (inputLength < 8)
            {
                string zeros = new String('0', 8 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 1, zeros);
                inputLength = barcodeData.Length;
            }

            for (int i = 1; i < 8; i++)
                count += i * (int)(barcodeData[i] - '0');

            checkValue = count % 11;
            if (checkValue == 11)
                checkValue = 0;

            char checkDigit = (char)(checkValue + '0');
            if (checkDigit == 'A')
                throw new InvalidDataException("PNZ: Invalid characters in input data.");

            barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
            inputLength = barcodeData.Length;
            Code39();
            barcodeText = "PNZ - " + new String(barcodeData, 1, inputLength - 1);
        }

        private void Logmars()
        {
            int inputLength = barcodeData.Length;
            if (inputLength > 59)
                throw new InvalidDataLengthException("LOGMARS: Input data too long.");

            Code39();
        }

        // Vehicle Identification Number (VIN).
        private void VINCode()
        {
            // This code verifies the check digit present in North American VIN codes.
            char inputCheckDigit;
            char outputCheckDigit;
            int[] value = new int[17];
            int[] weight = new int[] { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };
            int inputLength = barcodeData.Length;
            int sum;

            // Check length
            if (inputLength > 17)
                throw new InvalidDataLengthException("VIN Code: Input data too long.");

            // Pad with zeros
            if (inputLength < 17)
            {
                string zeros = new String('0', 17 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 1, zeros);
                inputLength = barcodeData.Length;
            }

            // Check input characters, I, O and Q are not allowed
            for (int i = 0; i < inputLength; i++)
            {
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);  // Make sure all characters are uppercase.
                if (CharacterSets.VINSet.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("VIN Code: Invalid data in input.");
            }

            inputCheckDigit = barcodeData[8];

            for (int i = 0; i < 17; i++)
            {
                if (char.IsDigit(barcodeData[i]))
                    value[i] = barcodeData[i] - '0';

                if ((barcodeData[i] >= 'A') && (barcodeData[i] <= 'I'))
                    value[i] = (barcodeData[i] - 'A') + 1;

                if ((barcodeData[i] >= 'J') && (barcodeData[i] <= 'R'))
                    value[i] = (barcodeData[i] - 'J') + 1;

                if ((barcodeData[i] >= 'S') && (barcodeData[i] <= 'Z'))
                    value[i] = (barcodeData[i] - 'S') + 2;
            }

            sum = 0;
            for (int i = 0; i < 17; i++)
                sum += value[i] * weight[i];

            outputCheckDigit = (char)('0' + (sum % 11));

            if (outputCheckDigit == ':')    // Check digit was 10
                outputCheckDigit = 'X';

            if (inputCheckDigit != outputCheckDigit)
                throw new InvalidDataException("VIN Code: Invalid check digit in input data.");

            Code39();
            barcodeText = new string(barcodeData);

        }

        private void Code39Extended()
        {
            int inputLength = barcodeData.Length;

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] > 127)
                    throw new InvalidDataException("Code 39 Extended: Invalid characters in input data.");
            }

            List<char> extendedData = new List<char>();
            for (int i = 0; i < inputLength; i++)
            {
                string extendedString = ExtendedC39Ctrl[barcodeData[i]];
                for (int l = 0; l < extendedString.Length; l++)
                    extendedData.Add(extendedString[l]);
            }

            barcodeData = extendedData.ToArray();
            Code39();
        }

        private void Code39()
        {
            int index;
            char checkDigit;
            int inputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (inputLength > 74)
                throw new InvalidDataLengthException("Code 39: Input data too long.");

            for (int i = 0; i < inputLength; i++)
            {
                if (CharacterSets.Code39Set.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Code 39: Invalid characters in input data.");
            }

            rowPattern.Append(Code39Table[43]);
            for (int i = 0; i < inputLength; i++)
            {
                index = CharacterSets.Code39Set.IndexOf(barcodeData[i]);
                rowPattern.Append(Code39Table[index]);
            }

            if (optionalCheckDigit)
            {
                checkDigit = CheckSum.Mod43CheckDigit(barcodeData);
                index = CharacterSets.Code39Set.IndexOf(checkDigit);
                rowPattern.Append(Code39Table[index]);
                checkDigitText = checkDigit.ToString();
            }

            rowPattern.Append(Code39Table[43]);

            if (symbolId == Symbology.LOGMARS || encodingMode == EncodingMode.HIBC)
                rowPattern.Replace('2', '3');

            barcodeText = barcodeMessage;

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}