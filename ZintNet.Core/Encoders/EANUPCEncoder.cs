/* EANUPCEncoder.cs Handles EAN UPC & ISMB 1D symbols */

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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class EANUPCEncoder : SymbolEncoder
    {
        # region Tables and Constants
        /*LEFT-HAND ENCODING        RIGHT-HAND ENCODING 
            ODD PARITY (A) EVEN PARITY (B) ALL CHARACTERS 
        0   0001101        0100111         1110010 
        1   0011001        0110011         1100110 
        2   0010011        0011011         1101100 
        3   0111101        0100001         1000010 
        4   0100011        0011101         1011100 
        5   0110001        0111001         1001110 
        6   0101111        0000101         1010000 
        7   0111011        0010001         1000100 
        8   0110111        0001001         1001000 
        9   0001011        0010111         1110100 */

        private const char NORMAL = 'n';
        private const char GUARD = 'g';
        private const char SUPPLEMENT = 's';
        private const string ISBNprefix = "978";

        // Encoding for righthand and lefthand odd.
        private static string[] RHandLHandOddTable = {
            "3211", "2221", "2122", "1411", "1132",
            "1231", "1114", "1312", "1213", "3112" };

        // Encoding for lefthand even.
        private static string[] LeftHandEvenTable = {
            "1123", "1222", "2212", "1141", "2311",
            "1321", "4111", "2131", "3121", "2113" };

        // Parity Data '0' = even '1' = odd.
        private static string[] EanParityTable = {
            "111111", "110100", "110010", "110001", "101100",
            "100110", "100011", "101010", "101001", "100101" };

        // Parity Data '0' = even '1' = odd.
        private static string[] UpcParityTable = {
            "000111", "001011", "001101", "001110", "010011",
            "011001", "011100", "010101", "010110", "011010" };

        // Plus 2 Parity  '0' = even '1' = odd.
        private static string[] Plus2Parity = {
            "11", "10", "01", "00" };

        // Plus 5 Parity  '0' = even '1' = odd.
        private static string[] Plus5Parity = {
            "00111", "01011", "01101", "01110", "10011",
            "11001", "11100", "10101", "10110", "11010" };
        # endregion

        StringBuilder binaryString;
        private string supplementMessage;
        private bool hasSupplementSymbol;
        private int linearWidth = 0;    // Linear width of the sysmbol excluding Supplement.
        private int offSet = 0;         // Offset applied to symbol if is composite.

        public EANUPCEncoder(Symbology symbology, string barcodeMessage, string supplementMessage, string compositeMessage, CompositeMode compositeMode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.supplementMessage = supplementMessage;
            this.compositeMessage = compositeMessage;
            this.compositeMode = compositeMode;
            if (!string.IsNullOrEmpty(supplementMessage))
                hasSupplementSymbol = true;

            if (!string.IsNullOrEmpty(compositeMessage))
                isCompositeSymbol = true;

            elementsPerCharacter = 7;
            offSet = (isCompositeSymbol) ? 1 : 0;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            if (symbolId != Symbology.ISBN)
                barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);

            switch (symbolId)
            {
                case Symbology.ISBN:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage.Replace('-', ' '));
                    isCompositeSymbol = false;
                    ISBN();
                    break;

                case Symbology.EAN13:
                    EAN13();
                    break;

                case Symbology.EAN8:
                    EAN8();
                    break;

                case Symbology.UPCA:
                    UPCA();
                    break;

                case Symbology.UPCE:
                    UPCE();
                    break;
            }

            if (hasSupplementSymbol)
                EanUpcSuppliment();

            // Expand the row pattern into the symbol data.
            int width = offSet;
            for (int i = 0; i < binaryString.Length; i++)
            {
                char value = binaryString[i];
                if (value == NORMAL || value == GUARD || value == SUPPLEMENT)
                {
                    width++;
                    if (value == SUPPLEMENT)
                        width += supplementMargin;
                }

                else
                    width += binaryString[i] - '0';
            }

            byte[] rowData = new Byte[width];
            bool latch = true;
            int position = offSet;
            for (int i = 0; i < binaryString.Length; i++)
            {
                char value = binaryString[i];
                // Suppliment switch.
                if (value == SUPPLEMENT)
                {
                    // Skip 12 elements for the supplement margin.
                    position += supplementMargin;
                    rowData[position++] = 4;
                    latch = true;
                }

                // Guard bar switch.
                else if (value == GUARD)
                    rowData[position++] = 3;

                // Normal bar switch.
                else if (value == NORMAL)
                    rowData[position++] = 2;

                else
                {
                    value -= '0';
                    for (int j = 0; j < value; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }
            }

            SymbolData symbolData = new SymbolData(rowData);
            Symbol.Add(symbolData);

            if (isCompositeSymbol)
            {
                // Separator pattern.
                latch = true;
                for (int rows = 0; rows < 3; rows++)
                {
                    offSet = (latch) ? 1 : 0;
                    rowData = new byte[linearWidth + 2];
                    rowData[0 + offSet] = 1;
                    rowData[(linearWidth + 1) - offSet] = 1;
                    symbolData = new SymbolData(rowData, 2);
                    Symbol.Insert(0, symbolData);
                    latch = !latch;
                }

                CompositeEncoder.AddComposite(symbolId, compositeMessage, Symbol, compositeMode, linearWidth);
            }

            return Symbol;
        }

        private void ISBN()
        {
            char checkDigit;
            int inputLength = barcodeData.Length;

            if (inputLength == 9 || inputLength == 10)
            {
                // Catch any invalid characters.
                for (int i = 0; i < inputLength; i++)
                {
                    if (!char.IsDigit(barcodeData[i]) && char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture) != 'X')  // Can have 'X' as check digit.
                        throw new InvalidDataException("ISBN-10: Invalid data in input");
                }

                if (inputLength == 10)
                {
                    // Confirm supplied a valid check digit, then discard it.
                    char cd = char.ToUpper(barcodeData[inputLength - 1], CultureInfo.CurrentCulture);
                    Array.Resize(ref barcodeData, inputLength - 1);
                    checkDigit = CheckSum.Mod11CheckDigit(barcodeData);
                    if (checkDigit != cd)
                        throw new InvalidDataException("ISBN-10: Invalid check digit.");
                }

                barcodeData = ArrayExtensions.Insert(barcodeData, 0, ISBNprefix);
            }

            else if (inputLength == 12 || inputLength == 13)
            {
                // Catch any invalid characters.
                for (int i = 0; i < inputLength; i++)
                {
                    if (!char.IsDigit(barcodeData[i]))
                        throw new InvalidDataException("ISBN-13: Invalid data in input");
                }

                if (inputLength == 13)
                {
                    char cd = barcodeData[inputLength - 1];
                    Array.Resize(ref barcodeData, inputLength - 1);
                    checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
                    if (checkDigit != cd)
                        throw new InvalidDataException("ISBN-13: Invalid check digit.");
                }
            }

            else
                throw new InvalidDataLengthException("ISBN: Invalid number of characters in the input data.");

            EAN13();
        }

        private void EAN13()
        {
            int dataValue;
            string parity;
            char parityBit;
            char checkDigit;

            int inputLength = barcodeData.Length;
            binaryString = new StringBuilder();

            if (inputLength == 12)
            {
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
                barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
                inputLength = barcodeData.Length;
            }

            else if (inputLength == 13)
            {
                // Confirm supplied a valid check digit.
                char cd = barcodeData[inputLength - 1];
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData, inputLength - 1);

                if (checkDigit != cd)
                    throw new InvalidDataException("EAN-13: Invalid check digit.");
            }

            else
                throw new InvalidDataLengthException("EAN-13: Requires 13 characters in the input data.");

            binaryString.Append("g111n");	// Add the left guard bars with tall and short switches.

            dataValue = barcodeData[0] - '0';
            parity = EanParityTable[dataValue];	// Get the parity based on the first barcodeData character.

            for (int i = 1; i < inputLength; i++)
            {
                dataValue = barcodeData[i] - '0';
                if (i < 7)
                {
                    parityBit = parity[i - 1];
                    if (parityBit == '0')	// Even.
                        binaryString.Append(LeftHandEvenTable[dataValue]);

                    else	// Odd.
                        binaryString.Append(RHandLHandOddTable[dataValue]);
                }

                if (i == 7)
                    binaryString.Append("g11111n");	// Insert the center guard bars with guard and normal switches.

                if (i > 6)
                    binaryString.Append(RHandLHandOddTable[dataValue]);
            }

            binaryString.Append("g111"); // Append the right guard bars with the tall switch.

            // Human readable text.
            barcodeText = new string(barcodeData);
            leftHandCharacter = barcodeText.Substring(0, 1);
            leftHandText = barcodeText.Substring(1, 6);
            rightHandText = barcodeText.Substring(7, 6);
            if(String.IsNullOrEmpty(supplementMessage))
                rightHandCharacter = ">";

            linearWidth = 95;
        }

        private void EAN8()
        {
            int dataValue;
            char checkDigit;

            int inputLength = barcodeData.Length;
            binaryString = new StringBuilder();

            if (inputLength == 7)
            {
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
                barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
                inputLength = barcodeData.Length;
            }

            else if (inputLength == 8)
            {
                // Confirm supplied a valid check digit.
                char cd = barcodeData[inputLength - 1];
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData, inputLength - 1);

                if (checkDigit != cd)
                    throw new InvalidDataException("EAN-8: Invalid check digit.");
            }

            else
                throw new InvalidDataLengthException("EAN-8: Requires 8 characters in the input data.");

            // Encode the barcodeData.
            binaryString.Append("g111n");	// Left guard bars.

            for (int i = 0; i < inputLength; i++)
            {
                dataValue = barcodeData[i] - '0';
                binaryString.Append(RHandLHandOddTable[dataValue]);
                if (i == 3)
                    binaryString.Append("g11111n");	// The center guard bars.
            }

            binaryString.Append("g111"); // End guard bars.

            barcodeText = new string(barcodeData);
            leftHandCharacter = "<";
            leftHandText = barcodeText.Substring(0, 4);
            rightHandText = barcodeText.Substring(4, 4);
            if (String.IsNullOrEmpty(supplementMessage))
                rightHandCharacter = ">";
            linearWidth = 67;
        }

        private void UPCA()
        {
            int dataValue;
            char checkDigit;

            int inputLength = barcodeData.Length;
            binaryString = new StringBuilder();

            if (inputLength == 11)
            {
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
                barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
                inputLength = barcodeData.Length;
            }

            else if (inputLength == 12)
            {
                // Confirm supplied a valid check digit.
                char cd = barcodeData[inputLength - 1];
                checkDigit = CheckSum.Mod10CheckDigit(barcodeData, inputLength - 1);

                if (checkDigit != cd)
                    throw new InvalidDataException("UPC-A: Invalid check digit.");
            }

            else
                throw new InvalidDataLengthException("UPC-A: Requires 12 characters in the input data.");

            // Encode the barcodeData.
            binaryString.Append("g111");	// Add the left guard bars.

            for (int i = 0; i < inputLength; i++)
            {
                dataValue = barcodeData[i] - '0';
                binaryString.Append(RHandLHandOddTable[dataValue]);
                if (i == 0)
                    binaryString.Append("n");		// Normal switch.

                if (i == 5)
                    binaryString.Append("g11111n");	// Insert the center guard bars.

                if (i == 10)
                    binaryString.Append("g");		// Guard switch.		
            }

            binaryString.Append("111"); // End guard bars.

            barcodeText = new string(barcodeData);
            leftHandCharacter = barcodeText.Substring(0, 1);
            leftHandText = barcodeText.Substring(1, 5);
            rightHandText = barcodeText.Substring(6, 5);
            rightHandCharacter = barcodeText.Substring(11, 1);
            linearWidth = 95;
        }

        private void UPCE()
        {
            string parity;
            char parityBit;
            char checkDigit;
            int dataValue;

            int inputLength = barcodeData.Length;
            binaryString = new StringBuilder();

            if (inputLength == 6)
                checkDigit = ExpandToUPCA(inputLength);

            else if (inputLength == 7)
            {
                // Confirm supplied a valid check digit.
                char cd = barcodeData[inputLength - 1];
                inputLength--;
                checkDigit = ExpandToUPCA(inputLength);
                if (checkDigit != cd)
                    throw new InvalidDataException("UPC-A: Invalid check digit.");

                Array.Resize(ref barcodeData, inputLength);
            }

            else
                throw new InvalidDataLengthException("UPC-E: Requires 7 characters in the input data.");

            // Encode the barcodeData.
            parity = UpcParityTable[checkDigit - '0'];
            binaryString.Append("g111n");		// Add the left guard bars.

            for (int i = 0; i < inputLength; i++)
            {
                dataValue = barcodeData[i] - '0';
                parityBit = parity[i];
                if (parityBit == '0')	// Even.
                    binaryString.Append(LeftHandEvenTable[dataValue]);

                else					// Odd.
                    binaryString.Append(RHandLHandOddTable[dataValue]);
            }

            binaryString.Append("g111111");	// Append the right guard bars.

            // Set the human readable text.
            barcodeText = new string(barcodeData);
            leftHandCharacter = "0";
            leftHandText = barcodeText;
            rightHandText = String.Empty;
            rightHandCharacter = checkDigit.ToString();
            linearWidth = 51;
        }

        /// <summary>
        /// Expands the UPC-E barcodeData to UPC-A for checksum calculation.
        /// </summary>
        /// <param name="inputLength">barcode data length</param>
        /// <returns>check sum</returns>
        private char ExpandToUPCA(int inputLength)
        {
            StringBuilder upcaCode = new StringBuilder();
            char[] upceData = new char[inputLength];

            Array.Copy(barcodeData, upceData, inputLength);
            upceData = ArrayExtensions.Insert(upceData, 0, '0');

            switch (upceData[6])
            {
                case '0':
                    upcaCode.Append(upceData, 0, 3);
                    upcaCode.Append("00000");
                    upcaCode.Append(upceData, 3, 3);
                    break;

                case '1':
                case '2':
                    upcaCode.Append(upceData, 0, 3);
                    upcaCode.Append(upceData, 6, 1);
                    upcaCode.Append("0000");
                    upcaCode.Append(upceData, 3, 3);
                    break;

                case '3':
                    upcaCode.Append(upceData, 0, 4);
                    upcaCode.Append("00000");
                    upcaCode.Append(upceData, 4, 2);
                    break;

                case '4':
                    upcaCode.Append(upceData, 0, 5);
                    upcaCode.Append("00000");
                    upcaCode.Append(upceData, 5, 1);
                    break;

                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    upcaCode.Append(upceData, 0, 6);
                    upcaCode.Append("0000");
                    upcaCode.Append(upceData, 6, 1);
                    break;
            }

            return CheckSum.Mod10CheckDigit(upcaCode.ToString().ToCharArray());
        }

        /// <summary>
        /// Appends the 2 or 5 digit supplement to this existing barcode.
        /// </summary>
        private void EanUpcSuppliment()
        {
            char[] supplimentData = MessagePreProcessor.NumericOnlyParser(supplementMessage);
            int inputLength = supplimentData.Length;

            switch (inputLength)
            {
                case 2:
                    AddPlus2(supplimentData);
                    break;

                case 5:
                    AddPlus5(supplimentData);
                    break;

                default:
                    throw new InvalidDataLengthException("EAN/UPC Suppliment: Invalid number of characters in the input data.");
            }

            supplementText = supplementMessage;
        }

        private void AddPlus2(char[] supplimentData)
        {
            int dataValue;
            string parity;
            char parityBit;
            int parityValue;
            int inputLength = supplimentData.Length;

            parityValue = (Int32.Parse(supplementMessage, CultureInfo.CurrentCulture)) % 4;
            parity = Plus2Parity[parityValue];
            binaryString.Append("s112");
            for (int i = 0; i < inputLength; i++)
            {
                dataValue = supplimentData[i] - '0';
                parityBit = parity[i];
                if (parityBit == '0')	// Even.
                    binaryString.Append(LeftHandEvenTable[dataValue]);

                else					// Odd.
                    binaryString.Append(RHandLHandOddTable[dataValue]);

                if (i < inputLength - 1)
                    binaryString.Append("11");
            }

            supplementBars = 20;
        }

        private void AddPlus5(char[] supplimentData)
        {
            int factor = 3;
            int weightTotal = 0;
            int checkValue;
            string parity;
            int dataValue;
            char parityBit;
            int inputLength = supplimentData.Length;

            // Get the check character.
            for (int i = inputLength - 1; i >= 0; i--)
            {
                weightTotal += (supplimentData[i] - '0') * factor;
                factor = (factor == 3) ? 9 : 3;
            }

            checkValue = weightTotal % 10;
            parity = Plus5Parity[checkValue];
            binaryString.Append("s112");
            for (int i = 0; i < inputLength; i++)
            {
                dataValue = supplimentData[i] - '0';
                parityBit = parity[i];
                if (parityBit == '0')	// Even.
                    binaryString.Append(LeftHandEvenTable[dataValue]);

                else					// Odd.
                    binaryString.Append(RHandLHandOddTable[dataValue]);

                if (i < inputLength - 1)
                    binaryString.Append("11");
            }

            supplementBars = 47;
        }
    }
}