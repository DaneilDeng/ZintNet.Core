/* Mailmark.cs - Handles Royal Mail 4-State Barcode */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2018 Milton Neal <milton200954@gmail.com>
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
    internal class MailmarkEncoder : SymbolEncoder
    {
        #region Tables
        // Allowed character values from Table 3
        private const string SetF = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string SetL = "ABDEFGHJLNPQRSTUWXYZ";
        private const string SetN = "0123456789";
        private const string SetS = " ";

        private static string[] postcodeFormat = new string[] {
            "FNFNLLNLS", "FFNNLLNLS", "FFNNNLLNL", "FFNFNLLNL", "FNNLLNLSS", "FNNNLLNLS" };

        // Data/Check Symbols from Table 5
        private static byte[] dataSymbolOdd = new byte[] {
            0x01, 0x02, 0x04, 0x07, 0x08, 0x0B, 0x0D, 0x0E, 0x10, 0x13, 0x15, 0x16,
            0x19, 0x1A, 0x1C, 0x1F, 0x20, 0x23, 0x25, 0x26, 0x29, 0x2A, 0x2C, 0x2F,
            0x31, 0x32, 0x34, 0x37, 0x38, 0x3B, 0x3D, 0x3E };

        private static byte[] dataSymbolEven = new byte[] {
            0x03, 0x05, 0x06, 0x09, 0x0A, 0x0C, 0x0F, 0x11, 0x12, 0x14, 0x17, 0x18,
            0x1B, 0x1D, 0x1E, 0x21, 0x22, 0x24, 0x27, 0x28, 0x2B, 0x2D, 0x2E, 0x30,
            0x33, 0x35, 0x36, 0x39, 0x3A, 0x3C };

        private static byte[] extenderGroupC = new byte[] {
            3, 5, 7, 11, 13, 14, 16, 17, 19, 0, 1, 2, 4, 6, 8, 9, 10, 12, 15, 18, 20, 21 };

        private static byte[] extenderGroupL = new byte[] {
            2, 5, 7, 8, 13, 14, 15, 16, 21, 22, 23, 0, 1, 3, 4, 6, 9, 10, 11, 12, 17, 18, 19, 20, 24, 25 };

        #endregion

        public MailmarkEncoder(Symbology symbolId, string barcodeMessage)
        {
            // Ensure postcode type 7 ends in 5 spaces.
            barcodeMessage = barcodeMessage.TrimEnd(new char[] {' '});
            if (barcodeMessage.EndsWith("XY11", StringComparison.CurrentCulture))
                barcodeMessage += "     ";

            this.barcodeMessage = barcodeMessage;
            this.symbolId = symbolId;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
            Mailmark();
            return Symbol;
        }

        private void Mailmark()
        {
            int inputLength = barcodeData.Length;
            char barcodeType = 'C';
            int formatId = 0;
            int versionId = 0;
            int classId = 0;
            int supplyChainId = 0;
            int itemId = 0;
            string postcode = String.Empty;
            int postcodeType = 0;
            string pattern = String.Empty;
            short[] destinationPostcode = new short[112];
            short[] aRegister = new short[112];
            short[] bRegister = new short[112];
            short[] tempRegister = new short[112];
            short[] cdvRegister = new short[112];
            byte[] data = new byte[26];
            int dataTop, dataStep;
            byte[] check = new byte[7];
            short[] extender = new short[27];
            int checkCount;
            StringBuilder barPattern = new StringBuilder();
            bool result;

            if (inputLength > 26)
                throw new InvalidDataLengthException("Mailmark: Input data too long.");

            if (inputLength <= 22)
            {
                for (int i = inputLength; i < 22; i++)
                    barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, ' ');

                inputLength = barcodeData.Length;
                barcodeType = 'C';
            }


            if (inputLength > 22 && inputLength <= 26)
            {
                for (int i = inputLength; i < 26; i++)
                    barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, ' ');

                inputLength = barcodeData.Length;
                barcodeType = 'L';
            }

            for (int i = 0; i < inputLength; i++)
            {
                barcodeData[i] = Char.ToUpper(barcodeData[i], CultureInfo.CurrentCulture);  // Make sure all characters are uppercase.
                if (CharacterSets.Mailmark.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Mailmark: Invalid character in input data.");
            }

            // Format is in the range 0-4.
            result = int.TryParse((new string(barcodeData, 0, 1)), out formatId);
            if (!result || formatId < 0 || formatId > 4)
                throw new InvalidDataException("Mailmark: Invalid Format character in input data.");

            // Version ID is in the range 1-4.
            result = int.TryParse((new string(barcodeData, 1, 1)), out versionId);
            versionId--;    // Internal field value of 0-3.
            if (!result || versionId < 0 || versionId > 3)
                throw new InvalidDataException("Mailmark: Invalid Version ID character in input data.");

            // Class is in the range 0-9 & A-E.
            result = int.TryParse((new string(barcodeData, 2, 1)), NumberStyles.HexNumber, null, out classId);
            if (!result || classId < 0 || classId > 14)
                throw new InvalidDataException("Mailmark: Invalid Class ID character in input data.");

            // Supply Chain ID is 2 digits for barcode C and 6 digits for barcode L
            int supplyChainLength = (barcodeType == 'C') ? 2 : 6;
            result = int.TryParse(new string(barcodeData, 3, supplyChainLength), out supplyChainId);
            if(!result)
                throw new InvalidDataException("Mailmark: Invalid Supply Chain ID character in input data.");

            // Item ID is 8 digits.
            result = int.TryParse(new string(barcodeData, 3 + supplyChainLength, 8), out itemId);
            if(!result)
                throw new InvalidDataException("Mailmark: Invalid Item ID character in input data.");

            // Seperate Destination Post Code plus DPS field.
            postcode = new string(barcodeData, 3 + supplyChainLength + 8, 9);

            // Detect postcode type
            /* Postcode type is used to select which format of postcode 
             * 
             * 1 = FNFNLLNLS
             * 2 = FFNNLLNLS
             * 3 = FFNNNLLNL
             * 4 = FFNFNLLNL
             * 5 = FNNLLNLSS
             * 6 = FNNNLLNLS
             * 7 = International designation*/

            if (postcode == "XY11     ")
                postcodeType = 7;

            else
            {
                if (postcode[7] == ' ')
                    postcodeType = 5;

                else
                {
                    if (postcode[8] == ' ')
                    {
                        // Types 1, 2 and 6
                        if (Char.IsDigit(postcode[1]))
                        {
                            if (Char.IsDigit(postcode[2]))
                                postcodeType = 6;

                            else
                                postcodeType = 1;
                        }

                        else
                            postcodeType = 2;
                    }

                    else
                    {
                        // Types 3 and 4
                        if (Char.IsDigit(postcode[3]))
                            postcodeType = 3;

                        else
                            postcodeType = 4;
                    }
                }
            }

            // Verify postcode type
            if (postcodeType != 7)
            {
                if (!VerifyPostcode(postcode, postcodeType))
                    throw new InvalidDataException("Mailmark: Invalid Postcode in input data.");
            }

            // Convert postcode to internal user field.
            if (postcodeType != 7)
            {
                pattern = postcodeFormat[postcodeType - 1];
                BinaryMath.BinaryLoad(bRegister, "0");
                for (int i = 0; i < 9; i++)
                {
                    switch (pattern[i])
                    {
                        case 'F':
                            BinaryMath.BinaryMultiply(bRegister, "26");
                            BinaryMath.BinaryLoad(tempRegister, "0");
                            for (int j = 0; j < 5; j++)
                            {
                                if ((SetF.IndexOf(postcode[i]) & (0x01 << j)) > 0)
                                    tempRegister[j] = 1;
                            }

                            BinaryMath.BinaryAdd(bRegister, tempRegister);
                            break;

                        case 'L':
                            BinaryMath.BinaryMultiply(bRegister, "20");
                            BinaryMath.BinaryLoad(tempRegister, "0");
                            for (int j = 0; j < 5; j++)
                            {
                                if ((SetL.IndexOf(postcode[i]) & (0x01 << j)) > 0)
                                    tempRegister[j] = 1;
                            }

                            BinaryMath.BinaryAdd(bRegister, tempRegister);
                            break;

                        case 'N':
                            BinaryMath.BinaryMultiply(bRegister, "10");
                            BinaryMath.BinaryLoad(tempRegister, "0");
                            for (int j = 0; j < 5; j++)
                            {
                                if ((SetN.IndexOf(postcode[i]) & (0x01 << j)) > 0)
                                    tempRegister[j] = 1;
                            }

                            BinaryMath.BinaryAdd(bRegister, tempRegister);
                            break;
                    }
                }

                // Destination postcode = accumulatorA + accumulatorB.
                BinaryMath.BinaryLoad(destinationPostcode, "0");
                BinaryMath.BinaryAdd(destinationPostcode, bRegister);

                BinaryMath.BinaryLoad(aRegister, "1");
                if (postcodeType == 1)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);

                BinaryMath.BinaryLoad(tempRegister, "5408000000");
                BinaryMath.BinaryAdd(aRegister, tempRegister);

                if (postcodeType == 2)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);

                BinaryMath.BinaryLoad(tempRegister, "5408000000");
                BinaryMath.BinaryAdd(aRegister, tempRegister);

                if (postcodeType == 3)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);

                BinaryMath.BinaryLoad(tempRegister, "54080000000");
                BinaryMath.BinaryAdd(aRegister, tempRegister);

                if (postcodeType == 4)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);

                BinaryMath.BinaryLoad(tempRegister, "140608000000");
                BinaryMath.BinaryAdd(aRegister, tempRegister);

                if (postcodeType == 5)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);

                BinaryMath.BinaryLoad(tempRegister, "208000000");
                BinaryMath.BinaryAdd(aRegister, tempRegister);

                if (postcodeType == 6)
                    BinaryMath.BinaryAdd(destinationPostcode, aRegister);
            }

            // Conversion from Internal User Fields to Consolidated Data Value
            // Set CDV to 0
            BinaryMath.BinaryLoad(cdvRegister, "0");

            // Add Destination Post Code plus DPS
            BinaryMath.BinaryAdd(cdvRegister, destinationPostcode);

            // Multiply by 100,000,000
            BinaryMath.BinaryMultiply(cdvRegister, "100000000");

            // Add Item ID
            BinaryMath.BinaryLoad(tempRegister, "0");
            for (int i = 0; i < 32; i++)

            {
                if ((0x01 & (itemId >> i)) > 0)
                    tempRegister[i] = 1;
            }

            BinaryMath.BinaryAdd(cdvRegister, tempRegister);

            if (barcodeType == 'C')
                BinaryMath.BinaryMultiply(cdvRegister, "100");  // Barcode C - Multiply by 100

            else
                BinaryMath.BinaryMultiply(cdvRegister, "1000000");  // Barcode L - Multiply by 1,000,000

            // Add Supply Chain ID
            //int scCount = barcodeType == 'C' ? 7 : 20;
            BinaryMath.BinaryLoad(tempRegister, "0");
            for (int i = 0; i < 20; i++)
            {
                if ((0x01 & (supplyChainId >> i)) > 0)
                    tempRegister[i] = 1;
            }

            BinaryMath.BinaryAdd(cdvRegister, tempRegister);

            // Multiply by 15
            BinaryMath.BinaryMultiply(cdvRegister, "15");

            // Add Class
            BinaryMath.BinaryLoad(tempRegister, "0");
            for (int i = 0; i < 4; i++)
            {
                if ((0x01 & (classId >> i)) > 0)
                    tempRegister[i] = 1;
            }

            BinaryMath.BinaryAdd(cdvRegister, tempRegister);
            // Multiply by 5
            BinaryMath.BinaryMultiply(cdvRegister, "5");

            // Add Format
            BinaryMath.BinaryLoad(tempRegister, "0");
            for (int i = 0; i < 4; i++)
            {
                if ((0x01 & (formatId >> i)) > 0)
                    tempRegister[i] = 1;
            }

            BinaryMath.BinaryAdd(cdvRegister, tempRegister);

            // Multiply by 4
            BinaryMath.BinaryMultiply(cdvRegister, "4");
            // Add Version ID
            BinaryMath.BinaryLoad(tempRegister, "0");
            for (int i = 0; i < 4; i++)
            {
                if ((0x01 & (versionId >> i)) > 0)
                    tempRegister[i] = 1;
            }

            BinaryMath.BinaryAdd(cdvRegister, tempRegister);
            if (barcodeType == 'C')
            {
                dataTop = 15;
                dataStep = 8;
                checkCount = 6;
            }

            else
            {
                dataTop = 18;
                dataStep = 10;
                checkCount = 7;
            }

            // Conversion from Consolidated Data Value to Data Numbers
            for (int i = 0; i < 112; i++)
                bRegister[i] = cdvRegister[i];

            for (int j = dataTop; j >= (dataStep + 1); j--)
            {
                for (int i = 0; i < 112; i++)
                {
                    cdvRegister[i] = bRegister[i];
                    bRegister[i] = 0;
                    aRegister[i] = 0;
                }

                aRegister[96] = 1;
                for (int i = 91; i >= 0; i--)
                {
                    bRegister[i] = BinaryMath.IsLarger(cdvRegister, aRegister);
                    if (bRegister[i] == 1)
                        BinaryMath.BinarySubtract(cdvRegister, aRegister);

                    BinaryMath.ShiftDown(aRegister);
                }

                data[j] = (byte)((cdvRegister[5] * 32) + (cdvRegister[4] * 16) + (cdvRegister[3] * 8) + (cdvRegister[2] * 4) + (cdvRegister[1] * 2) + cdvRegister[0]);
            }

            for (int j = dataStep; j >= 0; j--)
            {
                for (int i = 0; i < 112; i++)
                {
                    cdvRegister[i] = bRegister[i];
                    bRegister[i] = 0;
                    aRegister[i] = 0;
                }

                aRegister[95] = 1;
                aRegister[94] = 1;
                aRegister[93] = 1;
                aRegister[92] = 1;
                for (int i = 91; i >= 0; i--)
                {
                    bRegister[i] = BinaryMath.IsLarger(cdvRegister, aRegister);
                    if (bRegister[i] == 1)
                        BinaryMath.BinarySubtract(cdvRegister, aRegister);

                    BinaryMath.ShiftDown(aRegister);
                }

                data[j] = (byte)((cdvRegister[5] * 32) + (cdvRegister[4] * 16) + (cdvRegister[3] * 8) + (cdvRegister[2] * 4) + (cdvRegister[1] * 2) + cdvRegister[0]);
            }

            ReedSolomon.RSInitialise(0x25, checkCount, 1);
            ReedSolomon.RSEncode(dataTop + 1, data, check);
            // Append check digits to data.
            for (int i = 1; i <= checkCount; i++)
                data[dataTop + i] = check[checkCount - i];

            // Conversion from Data Numbers and Check Numbers to Data Symbols and Check Symbols
            for (int i = 0; i <= dataStep; i++)
                data[i] = dataSymbolEven[data[i]];

            for (int i = dataStep + 1; i <= (dataTop + checkCount); i++)
                data[i] = dataSymbolOdd[data[i]];

            // Conversion from Data Symbols and Check Symbols to Extender Groups
            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeType == 'C')
                    extender[extenderGroupC[i]] = data[i];

                else
                    extender[extenderGroupL[i]] = data[i];
            }

            // Conversion from Extender Groups to Bar Identifiers
            for (int i = 0; i < inputLength; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    switch (extender[i] & 0x24)
                    {
                        case 0x24:
                            barPattern.Append("F");
                            break;

                        case 0x20:
                            if (i % 2 > 0)
                                barPattern.Append("D");

                            else
                                barPattern.Append("A");

                            break;

                        case 0x04:
                            if (i % 2 > 0)
                                barPattern.Append("A");

                            else
                                barPattern.Append("D");

                            break;

                        default:
                            barPattern.Append("T");
                            break;
                    }

                    extender[i] = (short)(extender[i] << 1);
                }
            }

            BuildSymbol(barPattern);
            barcodeText = new string(barcodeData);
        }

        private void BuildSymbol(StringBuilder barPattern)
        {
            // Turn the symbol into a bar pattern.
            int index = 0;
            int patternLength = barPattern.Length;
            byte[] rowData1 = new byte[patternLength * 2];
            byte[] rowData2 = new byte[patternLength * 2];
            byte[] rowData3 = new byte[patternLength * 2];
            SymbolData symbolData;

            for (int i = 0; i < patternLength; i++)
            {
                if ((barPattern[i] == 'F') || (barPattern[i] == 'A'))
                    rowData1[index] = 1;

                rowData2[index] = 1;
                if ((barPattern[i] == 'F') || (barPattern[i] == 'D'))
                    rowData3[index] = 1;

                index += 2;
            }

            symbolData = new SymbolData(rowData1, 4.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2, 2.5f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData3, 4.0f);
            Symbol.Add(symbolData);
        }

        private static bool VerifyCharacter(char input, char type)
        {
            int value = -1;
            switch (type)
            {
                case 'F':
                    value = SetF.IndexOf(input);
                    break;

                case 'L':
                    value = SetL.IndexOf(input);
                    break;

                case 'N':
                    value = SetN.IndexOf(input);
                    break;

                case 'S':
                    value = SetS.IndexOf(input);
                    break;
            }

            return (value == -1) ? false : true;
        }

        private static bool VerifyPostcode(string postcode, int type)
        {
            bool result = true;
            char[] pattern = postcodeFormat[type - 1].ToCharArray();
            for (int i = 0; i < 9; i++)
            {
                if (!(VerifyCharacter(postcode[i], pattern[i])))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
