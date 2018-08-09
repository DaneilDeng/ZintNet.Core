/* Code93Encoder.cs - Handles Code 93 1D symbol */

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
    internal class Code93Encoder : SymbolEncoder
    {
        #region Tables
        private static string[] Code93Table = {
		    "131112","111213","111312","111411","121113","121212",
		    "121311","111114","131211","141111","211113","211212",
		    "211311","221112","221211","231111","112113","112212",
		    "112311","122112","132111","111123","111222","111321",
		    "121122","131121","212112","212211","211122","211221",
		    "221121","222111","112122","112221","122121","123111",
		    "121131","311112","311211","321111","112131","113121",
		    "211131","121221","312111","311121","122211","111141" };

        private static string[] C93Expanded = {
            "bU", "aA", "aB", "aC", "aD", "aE", "aF", "aG", "aH", "aI", "aJ", "aK",
            "aL", "aM", "aN", "aO", "aP", "aQ", "aR", "aS", "aT", "aU", "aV", "aW", "aX", "aY", "aZ",
            "bA", "bB", "bC", "bD", "bE", " ", "cA", "cB", "cC", "$", "%", "cF", "cG", "cH", "cI", "cJ",
            "+", "cL", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "cZ", "bF",
            "bG", "bH", "bI", "bJ", "bV", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "bK", "bL", "bM", "bN", "bO",
            "bW", "dA", "dB", "dC", "dD", "dE", "dF", "dG", "dH", "dI", "dJ", "dK", "dL", "dM", "dN", "dO",
            "dP", "dQ", "dR", "dS", "dT", "dU", "dV", "dW", "dX", "dY", "dZ", "bP", "bQ", "bR", "bS", "bT" };
        #endregion

        public Code93Encoder(Symbology symbology, string barcodeMessage)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
            Code93();
            return Symbol;
        }

        private void Code93()
        {
            int cWeight = 0;
            int kWeight = 0;
            int cCount = 1;
            int kCount = 2;
            int checkDigitC = 0;
            int checkDigitK = 0;

            StringBuilder inputBuffer = new StringBuilder();
            StringBuilder rowPattern = new StringBuilder();
            int inputLength = barcodeData.Length;

            // Check for valid characters and expand the input data.
            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] > 127)
                    throw new InvalidDataException("Code 93: Invalid characters in input data.");

                inputBuffer.Append(C93Expanded[barcodeData[i]]);
            }

            inputLength = inputBuffer.Length;
            if (inputLength > 107)
                throw new InvalidDataLengthException("Code 93: Input data too long.");

            int[] values = new int[inputLength];
            for (int i = 0; i < inputLength; i++)
                values[i] = CharacterSets.Code93Set.IndexOf(inputBuffer[i]);

            // Calculate the check characters starting from the right most position.
            for (int i = values.Length - 1; i >= 0; i--)
            {
                cWeight += (values[i] * cCount);
                cCount++;
                if (cCount == 21)
                    cCount = 1;

                kWeight += (values[i] * kCount);
                kCount++;
                if (kCount == 16)
                    kCount = 1;
            }

            checkDigitC = (cWeight % 47);
            kWeight += checkDigitC;
            checkDigitK = (kWeight % 47);

            rowPattern.Append(Code93Table[47]);	// Add the start character.
            for(int i = 0; i< inputLength; i++)
                rowPattern.Append(Code93Table[values[i]]);

            // Add the "C" and "K" check digits, stop character and termination bar
            rowPattern.Append(Code93Table[checkDigitC]);
            rowPattern.Append(Code93Table[checkDigitK]);
            rowPattern.Append(Code93Table[47] + "1");
            barcodeText = new string(barcodeData);
            checkDigitText += CharacterSets.Code93Set[checkDigitC];
            checkDigitText += CharacterSets.Code93Set[checkDigitK];

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);
        }
    }
}