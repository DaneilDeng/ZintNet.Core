/* PlesseyEncoder.cs - Handles UK Plessey and MSI Plessey 1D symbols */

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

using System.Collections.ObjectModel;
using System.Text;


namespace ZintNet.Core.Encoders
{
    /// <summary>
    /// Builds Plessey Symbols
    /// </summary>
    internal class PlesseyEncoder : SymbolEncoder
    {
        # region Tables
        private static string[] UKPlesseyTable = {
            "13131313", "31131313", "13311313", "31311313", "13133113", "31133113",
	        "13313113", "31313113", "13131331", "31131331", "13311331", "31311331",
            "13133131", "31133131", "13313131", "31313131", "31311331", "31311313"};

        private static string[] MSIPlesseyTable = {
            "12121212","12121221","12122112","12122121","12211212","12211221",
            "12212112","12212121","21121212","21121221","21","121"};
        # endregion

        MSICheckDigitType checkDigitType;

        public PlesseyEncoder(Symbology symbology, string barcodeMessage, MSICheckDigitType checkDigitType)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.checkDigitType = checkDigitType;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.MSIPlessey:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    MSIPlessey();
                    break;

                case Symbology.UKPlessey:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    UKPlessey();
                    break;
            }

            return Symbol;
        }

        private void MSIPlessey()
        {
            char checkDigit;
            int mod10Count = 0;
            int intputLength = barcodeData.Length;
            StringBuilder rowPattern = new StringBuilder();

            if (intputLength > 55)
                throw new InvalidDataLengthException("MSI Plessey: Input data too long.");

            // Set the human readable text before appending any check digits.
            barcodeText = barcodeMessage;

            // Use Mod 11 to calculate the optional check digit.
            // The Mod 11 check digit is calculated first.
            // If the check digit = 10 the message is deemed invalid.

            if (checkDigitType != MSICheckDigitType.None)
            {
                if (checkDigitType == MSICheckDigitType.Mod11Mod10 || checkDigitType == MSICheckDigitType.Mod10)
                    mod10Count = 1;

                else if (checkDigitType == MSICheckDigitType.Mod10Mod10)
                    mod10Count = 2;

                if (checkDigitType == MSICheckDigitType.Mod11 || checkDigitType == MSICheckDigitType.Mod11Mod10)
                {
                    // Mod 11 checksum. 
                    int weight = 0;
                    int weightFactor = 2;
                    int maxWeightFactor = 7;    // N.B. Weight Factor = 7 for IBM or 9 for NCR.

                    for (int i = intputLength - 1; i >= 0; i--)
                    {
                        int value = (int)(barcodeData[i] - '0');
                        weight += (value * weightFactor);
                        weightFactor++;
                        if (weightFactor > maxWeightFactor)
                            weightFactor = 2;
                    }

                    int checkValue = (int)(((11 - (weight % 11)) % 11));
                    if (checkValue == 10)
                    /*{
                        // Can't get this to work!!
                        // So just throw an exception instead.
                        barcodeData = ArrayEx.Insert(barcodeData, intputLength, "10");
                        checkDigitText += "10";
                    }*/
                        throw new InvalidDataException("MSI Plessey: Invalid data for Mod11 check sum.");

                    else
                    {
                        checkDigit = (char)(checkValue + '0');
                        checkDigitText += checkDigit.ToString();
                        barcodeData = ArrayExtensions.Insert(barcodeData, intputLength, checkDigit);
                        intputLength = barcodeData.Length;
                    }
                }

                if (mod10Count > 0)
                {
                    // Calculate the Mod 10 checksum.
                    // If using Mod10Mod10 loop through twice.
                    do
                    {
                        int weight = 0;
                        bool odd = true;
                        for (int i = intputLength - 1; i >= 0; i--)
                        {
                            int value = (int)(barcodeData[i] - '0');
                            if (odd)
                            {
                                value *= 2;
                                if (value > 9)
                                    value -= 9;
                            }

                            weight += value;
                            odd = !odd;
                        }

                        checkDigit = (char)(((10 - (weight % 10)) % 10) + '0');
                        barcodeData = ArrayExtensions.Insert(barcodeData, intputLength, checkDigit);
                        checkDigitText += checkDigit.ToString();
                        intputLength = barcodeData.Length;
                        mod10Count--;

                    } while (mod10Count > 0);
                }
            }


            // Add the start character.
            rowPattern.Append(MSIPlesseyTable[10]);
            for (int i = 0; i < intputLength; i++)
            {
                int value = CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]);
                rowPattern.Append(MSIPlesseyTable[value]);
            }

            // Add the stop character.
            rowPattern.Append(MSIPlesseyTable[11]);

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }

        private void UKPlessey()
        {
            int value;
            int intputLength = barcodeData.Length;
            byte[] checkBuffer = new byte[(intputLength * 4) + 8];
            byte[] grid = { 1, 1, 1, 1, 0, 1, 0, 0, 1 };
            StringBuilder rowPattern = new StringBuilder();

            if (intputLength > 65)
                throw new InvalidDataLengthException("UK Plessey: Input data too long.");

            for (int i = 0; i < intputLength; i++)
            {
                if (CharacterSets.UKPlesseySet.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("UK Plessey: Invalid data in input.");
            }

            barcodeText = barcodeMessage;

            // Start character.
            rowPattern.Append(UKPlesseyTable[16]);

            // Data.
            for (int i = 0; i < intputLength; i++)
            {
                value = CharacterSets.UKPlesseySet.IndexOf(barcodeData[i]);
                rowPattern.Append(UKPlesseyTable[value]);
                checkBuffer[4 * i] = (byte)(value & 1);
                checkBuffer[4 * i + 1] = (byte)((value >> 1) & 1);
                checkBuffer[4 * i + 2] = (byte)((value >> 2) & 1);
                checkBuffer[4 * i + 3] = (byte)((value >> 3) & 1);
            }

            // CRC value digit code adapted from code by Leonid A. Broukhis used in GNU Barcode.
            for (int i = 0; i < (4 * intputLength); i++)
            {
                if (checkBuffer[i] != 0)
                {
                    for (int j = 0; j < 9; j++)
                        checkBuffer[i + j] ^= grid[j];
                }
            }

            for (int i = 0; i < 8; i++)
            {
                switch (checkBuffer[intputLength * 4 + i])
                {
                    case 0:
                        rowPattern.Append("13");
                        break;

                    case 1:
                        rowPattern.Append("31");
                        break;
                }
            }

            rowPattern.Append("4");  // Termination code.
            rowPattern.Append(UKPlesseyTable[17]);  // Stop character.

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}