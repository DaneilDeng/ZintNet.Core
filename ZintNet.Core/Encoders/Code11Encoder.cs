/* Code11Encoder.cs - Handles Code 11 1D symbol */

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
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    /// <summary>
    /// Builds a Code 11 symbol.
    /// </summary>
    internal class Code11Encoder : SymbolEncoder
    {
        private static string[] Code11Table = {
		    "111131", "311131", "131131", "331111", "113131", "313111",
		    "133111", "111331", "311311", "311111", "113111", "113311" };

        private int numberOfCheckDigits;
        private bool optionalCheckDigit;

        public Code11Encoder(string barcodeMessage, bool optionalCheckDigit, int numberOfCheckDigits)
        {
            this.barcodeMessage = barcodeMessage;
            this.optionalCheckDigit = optionalCheckDigit;
            this.numberOfCheckDigits = numberOfCheckDigits;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
            Code11();
            return Symbol;
        }

        private void Code11()
        {
            int index;
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            if (inputLength > 121)
                throw new InvalidDataLengthException("Code 11: Input data too long.");

            // Catch any invalid characters.
            for (int i = 0; i < inputLength; i++)
            {
                if (CharacterSets.Code11Set.IndexOf(barcodeData[i]) == -1)
                    throw new InvalidDataException("Code 11: Invalid data in input.");
            }

            if (optionalCheckDigit)
            {
                // Calculate the "C" & "K" checksums.
                int cCount = 1;
                int kCount = 2;
                int cWeight = 0;
                int kWeight = 0;
                int checkDigitC = 0;
                int checkDigitK = 0;

                for (int i = inputLength - 1; i >= 0; i--)
                {
                    index = CharacterSets.Code11Set.IndexOf(barcodeData[i]);
                    cWeight += (index * cCount);
                    cCount++;
                    if (cCount == 11)
                        cCount = 1;

                    kWeight += (index * kCount);
                    kCount++;
                    if (kCount == 10)
                        kCount = 1;
                }

                checkDigitC = (cWeight % 11);
                kWeight += checkDigitC;

                checkDigitK = (kWeight % 11);
                checkDigitText = checkDigitText += CharacterSets.Code11Set[checkDigitC];

                // Using 2 check digits.
                if (numberOfCheckDigits == 2)
                    checkDigitText += CharacterSets.Code11Set[checkDigitK];
            }

            // Add the start character.
            rowPattern.Append(Code11Table[11]);
            for (int i = 0; i < inputLength; i++)
            {
                index = CharacterSets.Code11Set.IndexOf(barcodeData[i]);
                rowPattern.Append(Code11Table[index]);
            }

            for (int i = 0; i < checkDigitText.Length; i++)
            {
                index = CharacterSets.Code11Set.IndexOf(checkDigitText[i]);
                rowPattern.Append(Code11Table[index]);
            }

            // Add stop character.
            rowPattern.Append(Code11Table[11]);
            barcodeText = barcodeMessage;

            // Expand row into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}