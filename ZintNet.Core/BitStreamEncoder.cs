/* BitStreamEncoder.cs - Encodes the bit stream for DataBar Expanded and Composited data */

/*
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>

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
    ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
    LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
    OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
    SUCH DAMAGE.
 */

using System;

namespace ZintNet
{
    /// <summary>
    /// Generates the bit stream for Databar Expanded and Composite symbols.
    /// </summary>

    internal static class BitStreamEncoder
    {
        # region Tables and Constants

        const char FNC1 = '[';

        const int NUM_MODE = 1;
        const int ALNU_MODE = 2;
        const int ISO_MODE = 3;
        const int ALPH_MODE = 4;

        const int IS_NUM = 0x1;
        const int IS_FNC1 = 0x2;
        const int IS_ALNU = 0x4;
        const int IS_ISO = 0x8;

        static byte[] LookUp = { /* Byte look up table with IS_XXX bits */
	        /* 32 control characters: */
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
	        /* 32 punctuation and numeric characters: */
		        8,8,8,0,0,8,8,8,8,8,0xc,8,0xc,0xc,0xc,0xc,
		        0xd,0xd,0xd,0xd,0xd,0xd,0xd,0xd,0xd,0xd,
		        8,8,8,8,8,8,
	        /* 32 upper case and punctuation characters: (FNC1 = [ ) */
		        0,
		        0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,
		        0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,0xc,
		        0xf,0,0,0xc,8,
	        /* 32 lower case and punctuation characters: */
		        0,
		        8,8,8,8,8,8,8,8,8,8,8,8,8,
		        8,8,8,8,8,8,8,8,8,8,8,8,8,
		        0,0,0,0,0,
	        /* extended ASCII */
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };

        static int[][] CCATable = { new int[]{ 167, 138, 118, 108, 88, 78, 59 },
                                    new int[]{ 167, 138, 118, 98, 78 },
                                    new int[]{ 197, 167, 138, 108, 78} };

        static int[][] CCBTable = { new int[]{ 336, 296, 256, 208, 160, 104, 56 },
                                    new int[]{ 768, 648, 536, 416, 304, 208, 152, 112, 72, 32 },
                                    new int[]{ 1184, 1016, 840, 672, 496, 352, 264, 208, 152, 96, 56} };

        # endregion
       
        static int numericValue;
        static char alphaValue;
        static int inputLength;
        static int position;
        static int currentMode;
        static char[] inputData;
        static BitVector binaryData;

        /// <summary>
        /// Encodes the Databar message into a bit stream.
        /// </summary>
        /// <param name="symbology"></param>
        /// <param name="barcodeData"></param>
        /// <param name="isCompositeSymbol"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static BitVector DatabarExpBitStream(Symbology symbology, char[] barcodeData, bool isCompositeSymbol, int segments)
        {
            binaryData = new BitVector();
            inputLength = barcodeData.Length;
            inputData = new char[inputLength];
            Array.Copy(barcodeData, inputData, inputLength);  // Local copy.
 
            position = 0;
            int placeHolder = 0;

            string aiString;
            int weight = 0;
            int countryCode = 0;

            binaryData.AppendBit((isCompositeSymbol) ? (byte)1 : (byte)0);

            aiString = new string(inputData, 0, 2);
            if (inputLength >= 26)
            {
                // Possible weight field.
                if (!(int.TryParse(new string(inputData, 20, 6), out weight)))
                    weight = 0;
            }

	        // Look for AI [01].
            if (inputLength >= 16 && aiString == "01")
            {
                if (inputData[2] == '9')
                {
                    // Look for fixed length with AI 01[9] + 3103[0-32767].
                    if (inputLength == 26 && inputData[16] == '3' && inputData[17] == '1' && inputData[18] == '0'
                        && inputData[19] == '3' && weight <= 32767)
                    {
                        // Method 0100, AI's 01 + 3103.
                        binaryData.AppendBits(0x04, 4);
                        position += 3;                          // Skip AI 01 and PI 9.
                        Convert12();                            // Write PID-12.
                        binaryData.AppendBits(weight, 15);
                        position += 11;                         // Skip check digit & jump weight field.
                    }

                    // Look for fixed length with AI 01[9] + 3203202[0-009999].
                    else if (inputLength == 26 && inputData[16] == '3' && inputData[17] == '2' && inputData[18] == '0'
                        && inputData[19] == '2' && weight <= 9999)
                    {
                        // Method 0101, AI's 01 + 3202.
                        binaryData.AppendBits(0x5, 4);
                        position += 3;
                        Convert12();
                        binaryData.AppendBits(weight, 15);
                        position += 11;
                    }

                    // Look for fixed length with AI 01[9] + 3203[0-022767].
                    else if (inputLength == 26 && inputData[16] == '3' && inputData[17] == '2' && inputData[18] == '0'
                        && inputData[19] == '3' && weight <= 22767)
                    {
                        // Method 0101, AI's 01 + 3203.
                        binaryData.AppendBits(0x05, 4);
                        position += 3;
                        Convert12();
                        binaryData.AppendBits((int)(weight + 10000), 15);
                        position += 11;
                    }

                    // Look for AI 01[9] + 392[0-3].
                    else if (inputLength >= 21 && inputData[16] == '3' && inputData[17] == '9' && inputData[18] == '2' &&
                        (inputData[19] >= '0' && inputData[19] <= '3'))
                    {
                        // Method 01100, AI's 01 + 392x + G.P.
                        binaryData.AppendBits((0x0c << 2), 7);              // Write method + 2 VLS bits
                        position += 3;
                        Convert12();
                        binaryData.AppendBits(inputData[19] - '0', 2);      // Write D.P.
                        position += 5;                                      // Skip check digit & jump price AI.
                        placeHolder = 6;
                    }

                    // Look for AI 01[9] + 393[0-3].
                    else if (inputLength >= 24 && inputData[16] == '3' && inputData[17] == '9' && inputData[18] == '3' &&
                        (inputData[19] >= '0' && inputData[19] <= '3'))
                    {
                        // Method 01101, AI's 01 + 393x[NNN] + G.P.
                        binaryData.AppendBits((0x0d << 2), 7);
                        position += 3;
                        Convert12();
                        binaryData.AppendBits(inputData[19] - '0', 2);                      // Write D.P.
                        position += 5;                                                      // Skip check digit & jump price AI.
                        if (!(int.TryParse(new string(inputData, 20, 3), out countryCode))) // ISO country code.
                            countryCode = 0;

                        binaryData.AppendBits(countryCode, 10);
                        position += 3;                                                      // Jump ISO country code.
                        placeHolder = 6;
                    }

                    // Look for fixed length with AI 01[9] + 310x/320x[0-099999].
                    else if (inputLength == 26 && inputData[16] == '3' && (inputData[17] == '1' || inputData[17] == '2')
                        && inputData[18] == '0' && weight <= 99999)
                    {
                        // Methods 0111000-0111001, AI's 01 + 3x0x no date.
                        int value = 0x38 + (inputData[17] - '1');
                        binaryData.AppendBits(value, 7);
                        position += 3;
                        Convert12();
                        weight += (inputData[19] - '0') * 100000;           // Decimal digit.
                        binaryData.AppendBits((weight >> 16), 4);
                        binaryData.AppendBits((weight & 0xffff), 16);
                        position += 11;                                     // Skip check digit and jump weight field.
                        binaryData.AppendBits(38400, 16);                   // No date.
                    }

                    // look for fixed length + AI 01[9] + 310x/320x[0-099999] + 11/13/15/17.
                    else if (inputLength == 34 && inputData[16] == '3' && (inputData[17] == '1' || inputData[17] == '2')
                        && inputData[18] == '0' && weight <= 99999 && inputData[26] == '1' &&
                        (inputData[27] == '1' || inputData[27] == '3' || inputData[27] == '5' || inputData[27] == '7'))
                    {
                        // Methods 0111000-0111111, AI's 01 + 3x0x + 1x.
                        int value = 0x38 + (inputData[27] - '1') + (inputData[17] - '1');
                        binaryData.AppendBits(value, 7);
                        position += 3;
                        Convert12();
                        weight += (inputData[19] - '0') * 100000;
                        binaryData.AppendBits((weight >> 16), 4);
                        binaryData.AppendBits((weight & 0xffff), 16);
                        position += 13;
                        ConvertDate();  // Date.
                    }
                }

                else
                {
                    // Method 1 (plus 2-bit variable length symbol bit field), AI 01.
                    binaryData.AppendBits(1 << 2, 3);
                    position += 2;
                    Convert13();
                    position++; // Skip check digit.
                    placeHolder = 2;
                }
            }

            else
            {
                // Method 00 (plus 2-bit variable length symbol bit field), not AI 01.
                binaryData.AppendBits(0, 4);
                placeHolder = 3;
            }

	        currentMode = NUM_MODE;         // Start in numeric mode.
            while (position < inputLength)  // General purpose encodation.
            {
                switch (currentMode)
                {
                    case NUM_MODE:
                            ProcessNUMMode();
                            break;

                    case ALNU_MODE:
                            ProcessALNUMode();
                            break;

                    case ISO_MODE:
                            ProcessISOMode();
                            break;
                }
            }

            int remainder = 12 - (binaryData.SizeInBits % 12);
            if (remainder == 12)
                remainder = 0;

            if (binaryData.SizeInBits < 36)
                remainder = 36 - binaryData.SizeInBits;

            if (binaryData.SizeInBits > 252)
                throw new InvalidDataLengthException("Databar Expanded: Too many characters in the input data.");
            
            if (symbology == Symbology.DatabarExpandedStacked)
            {
                // Calculate how many data characters (segments) we need.
                // Last row of stacked symbol must have at least 2 segments (1 column).
                int dataCharacters = (binaryData.SizeInBits / 12) + 1;  // Make allowance for the check character.
                if (remainder > 0)
                    dataCharacters++;

                if ((dataCharacters % segments) == 1)   // Need one more!
                    remainder += 12;
            }

            // Now add padding to binary string (7.2.5.5.4)
            if(remainder != 0)
                AddPadding(binaryData.SizeInBits + remainder);

            // Patch variable length symbol bit field.
            if (placeHolder > 0)
            {
                byte d1;
                byte d2;
                d1 = (byte)(((binaryData.SizeInBits / 12) + 1) & 0x01);
                if (binaryData.SizeInBits <= 156)
                    d2 = 0;

                else
                    d2 = 1;

                binaryData[placeHolder] = d1;
                binaryData[placeHolder + 1] = d2;
            }

            return binaryData;
        }

        /// <summary>
        /// Encodes the composite symbol data into a binary bit stream.
        /// </summary>
        /// <param name="symbology">host symbolology</param>
        /// <param name="compositeData">data to be encoded</param>
        /// <param name="compositeMode">composite mode</param>
        /// <param name="dataColumns">number of data columns</param>
        /// <param name="eccLevel">error correction level</param>
        /// <param name="linearWidth">width of the host symbol</param>
        /// <returns></returns>
        public static BitVector CompositeBitStream(Symbology symbology, char[] compositeData, ref CompositeMode compositeMode,
            ref int dataColumns, ref int eccLevel, int linearWidth)
        {
            binaryData = new BitVector();
            inputLength = compositeData.Length;
            inputData = new char[inputLength];
            Array.Copy(compositeData, inputData, inputLength);  // Local copy.
            position = 0;

            string aiString = new string(inputData, 0, 2);
            if ((aiString == "11" | aiString == "17") && inputLength > 8)
            {
                // Encoding Method field "10" - date and lot number.
                // Production Date AI[11] or Expiration Date AI[17].

                position += 2;  // Skip over AI.
                binaryData.AppendBits(0x02, 2);
                ConvertDate();
                binaryData.AppendBit((aiString == "11") ? (byte)0 : (byte)1);

                // Check for an AI[10] after the date.
                if (inputLength > 10 && inputData[8] == '1' && inputData[9] == '0')
                    position += 2;          // Lot number follows, skip over the AI[10].

                else
                {
                    inputData[7] = FNC1;    // Need to insert FNC1 to indicate no lot number.
                    position--;
                }

                currentMode = NUM_MODE;
            }

            else if (aiString == "90" && TestForAI90())
            {
                binaryData.AppendBits(0x03, 2);
                ProcessAI90();
            }

            else
            {
                // Method 0.
                position = 0;
                binaryData.AppendBit(0);    // General purpose encodation method bit flag 0.
                currentMode = NUM_MODE;
            }

            while (position < inputLength)  // General purpose encodation.
            {
                switch (currentMode)
                {
                    case NUM_MODE:
                            ProcessNUMMode();
                            break;

                    case ALNU_MODE:
                            ProcessALNUMode();
                            break;

                    case ISO_MODE:
                            ProcessISOMode();
                            break;
                }
            }

            int targetBitSize = 0;
            if (compositeMode == CompositeMode.CCA)
            {
                targetBitSize = GetTargetSize(compositeMode, ref dataColumns, ref eccLevel, linearWidth);
                if (targetBitSize == 0)
                    compositeMode = CompositeMode.CCB;
            }

            if (compositeMode == CompositeMode.CCB)
            {
                targetBitSize = GetTargetSize(compositeMode, ref dataColumns, ref eccLevel, linearWidth);
                if (targetBitSize == 0)
                {
                    if (symbology == Symbology.Code128)
                        compositeMode = CompositeMode.CCC;

                    else
                        return null;
                }
            }

            if (compositeMode == CompositeMode.CCC)
            {
                targetBitSize = GetTargetSize(compositeMode, ref dataColumns, ref eccLevel, linearWidth);
                if (targetBitSize < binaryData.SizeInBits)
                    return null;
            }

            AddPadding(targetBitSize);
            return binaryData;
        }

        /// <summary>
        /// Test if AI[90] data compression can be used.
        /// </summary>
        /// <returns>true/false</returns>
        static bool TestForAI90()
        {
            // Possible method "11"
            // Must start with 0, 1, 2 or 3 digits follow by an upper case letter.
            position = 2;
            numericValue = 0;

            for (int i = 0; i < 4; i++)
            {
                if (char.IsDigit(inputData[position]))
                {
                    numericValue = (numericValue * 10) + (inputData[position] - '0');
                    position++;
                }

                else if (char.IsUpper(inputData[position]))
                {
                    alphaValue = inputData[position];
                    position++;
                    break;
                }

                else
                {
                    numericValue = -1;
                    break;
                }
            }

            return (numericValue >= 0);
        }

        /// <summary>
        /// Encodes the compressed part of AI[90] field into the binary bit stream.
        /// </summary>
        static void ProcessAI90()
        {
            // Method "11", look ahead to find best compaction scheme.
            string AlphaTable = "BDHIJKLNPQRSTVWZ";
            string nextAI;
            int aiValue = 0;
            int i;
            int j = 10000;      // 10000: initial flag for non-numeric index
            int alLessNu = 0;   // upper-case - digit, < -9000 if non-alnu seen
            for (i = position; i < inputLength; i++)
            {
                if ((inputData[i] == FNC1))
                    break; // At the end.

                if (j == 10000 && !char.IsDigit(inputData[i]))
                    j = i; // Keep first non-numeric index.

                if (char.IsDigit(inputData[i]))
                    alLessNu--;

                else if (char.IsUpper(inputData[i]))
                    alLessNu++;

                else
                    alLessNu = -10000; // Flag that char not digit or upper case.
            }

            if (i < inputLength)    // AI to follow.
            {
                if (inputData[i] == FNC1)
                {
                    nextAI = new string(inputData, i + 1, 2);
                    if (nextAI == "21")
                        aiValue = 2;

                    nextAI = new string(inputData, i + 1, 4);
                    if (nextAI == "8004")
                        aiValue = 3;
                }
            }

            else
                aiValue = 0;

            if (alLessNu > 0)
            {
                binaryData.AppendBits(0x03, 2);     // 11: alpha encoding.
                currentMode = ALPH_MODE;
            }

            else if (i > j && ((j - position) < 4))
            {
                binaryData.AppendBit(0);            // 0: alphanumeric encoding.
                currentMode = ALNU_MODE;
            }

            else
            {
                binaryData.AppendBits(0x02, 2);     // 10: numeric encoding.
                currentMode = NUM_MODE;
            }

            // next AI is 1 or 2 bit field
            if (aiValue == 0)
                binaryData.AppendBit(0);    // 0: no AI 21 or 8004

            else
                binaryData.AppendBits(aiValue, 2); // 10: AI 21 or 11: AI 8004.

            int letterValue = AlphaTable.IndexOf(alphaValue);

            if (numericValue < 31 && letterValue != -1)
            {
                // Encoding can be done according to 5.2.2 c) 2)
                binaryData.AppendBits(numericValue, 5); // Five bit binary string representing value before letter.
                binaryData.AppendBits(letterValue, 4);  // Followed by four bit representation of letter from letterValue.
            }

            else
            {
                // Encoding is done according to 5.2.2 c) 3)
                binaryData.AppendBits(0x1f, 5);
                binaryData.AppendBits(numericValue, 10);    // Ten bit representation of number.
                binaryData.AppendBits(alphaValue - 65, 5);  // Five bit representation of ASCII character.
            }

            EncodeAI90(aiValue);
            if (aiValue == 2)
                position += 2; // Skip "21"

            else if (aiValue == 3)
                position += 4; // Skip "8004"
        }
        /// <summary>
        /// Process the rest of the AI[90] field.
        /// </summary>
        /// <param name="aiValue"></param>
        private static void EncodeAI90(int aiValue)
        {
            while ((inputData[position - 1] != FNC1) && (inputData[position - 2] != FNC1) && (position < inputLength))
            {
                switch (currentMode)
                {
                    case NUM_MODE:
                        if (inputData[position] == FNC1)
                        {
                            if (aiValue == 2)   // Move up char after "21" in case it is needed for NUM_MODE
                                inputData[position + 1] = inputData[position + 3];

                            else if (aiValue == 3)  // Move up char after "8004" in case it is needed for NUM_MODE
                                inputData[position + 1] = inputData[position + 5];
                        }

                        ProcessNUMMode();
                        break;

                    case ALNU_MODE:
                        ProcessALNUMode();
                        break;

                    case ISO_MODE:
                        ProcessISOMode();
                        break;

                    case ALPH_MODE:
                        ProcessALPHMode();
                        break;

                }
            }
        }

        static void ProcessNUMMode()
        {
            char firstDigit, secondDigit;
            int first, second;

            // Check first char type.
            first = LookUp[(firstDigit = inputData[position])];
            
            if ((first & IS_NUM) == 0)
            {
                // First not a digit, latch to ALNU mode.
                binaryData.AppendBits(0, 4);
                currentMode = ALNU_MODE;
                return;
            }

            // Check for end.
            if ((position + 1) == inputLength && ((first & IS_FNC1) == 0))
            {
                // Single digit left, encode as digit & FNC1.
                binaryData.AppendBits(((firstDigit - '0') * 11) + 10 + 8, 7);
                position += 1;
                return;
            }

            else
                second = LookUp[(secondDigit = inputData[position + 1])];

            if (((first & second & IS_FNC1) != 0) || ((second & IS_NUM) == 0))
            {
                // Double FNC1 or 2nd character not a digit, latch to ALNU mode.
                binaryData.AppendBits(0, 4);
                currentMode = ALNU_MODE;
                return;
            }

            else
            {
                // Both digits, encode as 7-bits.
                position += 2;
                int d1, d2;
                if ((first & IS_FNC1) != 0)
                    d1 = 10;

                else
                    d1 = firstDigit - '0';

                if ((second & IS_FNC1) != 0)
                    d2 = 10;

                else
                    d2 = secondDigit - '0';

                binaryData.AppendBits(((d1 * 11) + d2 + 8), 7);
                currentMode = NUM_MODE;
                return;
            }
        }

        static void ProcessALNUMode()
        {
            char character;
            int first, next;
            int value;
            int i;

            // Check first char type.
            first = LookUp[(character = inputData[position])];
            
            if ((first & IS_ALNU) == 0)
            {
                // Not a ALNU, latch to ISO
                binaryData.AppendBits(0x04, 5);
                currentMode = ISO_MODE;
                return;
            }

            if ((position + 1) < inputLength)
            {
                if (((first & IS_NUM) != 0) && (((first | LookUp[inputData[position + 1]]) & IS_FNC1) == 0))
                {
                    // Next is NUM, look for more.
                    for (i = 1; i < 6; i++)
                    {
                        if (position + i == inputLength)    // At the end.
                        {
                            if (i >= 4)
                            {
                                // Latch numeric if >= 4 numbers at end.
                                binaryData.AppendBits(0, 3);
                                currentMode = NUM_MODE;
                                return;
                            }

                            else
                                break;  // Stay in ALNU
                        }

                        else
                        {
                            next = LookUp[inputData[position + i]];
                            if ((next & IS_NUM) == 0)
                                break;  // Stay in ALNU if < 6 digits coming up.
                        }
                    }

                    if (i == 6)
                    {
                        // If 6 or more digits coming up, latch to NUM.
                        binaryData.AppendBits(0, 3);
                        currentMode = NUM_MODE;
                        return;
                    }
                }
            }

            // Process character in ALNU mode.
            position++;
            if ((first & IS_NUM) != 0)
            {
                // FNC1 or 0-9
                if ((first & IS_FNC1) != 0)
                {
                    value = 0xf;
                    currentMode = NUM_MODE;
                }

                else
                    value = character - '0' + 5;

                binaryData.AppendBits(value, 5);
            }

            else
            {
                if (character >= 'A')
                    value = character - 'A';  // A-Z

                else if (character >= ',')
                    value = character - ',' + 0x1b;   // ,-./

                else
                    value = 0x1a;   // *

                binaryData.AppendBits(value + 0x20, 6);
            }
        }

        static void ProcessISOMode()
        {
            char character;
            int i, first, next, numberCount;
            int value;

            // Check first char type.
            first = LookUp[(character = inputData[position])];
            
            numberCount = 0;
            if (((first & IS_ALNU) != 0) && ((first & IS_FNC1) == 0))
            {
                // Next is ALNU (& not FNC1), look 9 for more ALNU.
                if ((first & IS_NUM) != 0)
                    numberCount = 1;    // Also count leading "digits".

                for (i = 1; i < 10; i++)
                {
                    if((position + i) == inputLength)   // At the end.
                    {
                        if ((numberCount >= 4) || (numberCount <= -4))
                        {
                            // Latch numeric if >= 4 numbers at end.
                            binaryData.AppendBits(0, 3);
                            currentMode = NUM_MODE;
                            return;
                        }

                        if (i >= 5)
                        {
                            // Latch ALNU if >= 5 alphanumbers at end.
                            binaryData.AppendBits(0x04, 5);
                            currentMode = ALNU_MODE;
                            return;
                        }

                        else
                            break;
                    }

                    else
                        next = LookUp[inputData[position + i]];

                    if ((next & IS_NUM) != 0)
                    {
                        if (numberCount > 0)
                            numberCount++;  // Count leading digits.
                    }

                    else if (numberCount > 0)
                        numberCount = -numberCount; // Stop counting if not a digit.

                    if ((next & IS_ALNU) == 0)
                        break;
                }

                if (i == 10)
                {
                    if ((numberCount >= 4) || (numberCount <= -4))
                    {
                        // Latch numeric if >= 4 numbers follow & no ISO chars in next 10.
                        binaryData.AppendBits(0, 3);
                        currentMode = NUM_MODE;
                        return;
                    }

                    else
                    {
                        // Latch ALNU if no ISO only chars in next 10.
                        binaryData.AppendBits(0x04, 5);
                        currentMode = ALNU_MODE;
                        return;
                    }
                }
            }

            // Process character in ISO mode.
            position += 1;
            if ((first & IS_NUM) != 0)
            {
                // FNC1 or 0-9
                if ((first & IS_FNC1) != 0)
                {
                    value = 0x0f;
                    currentMode = NUM_MODE;
                }

                else
                    value = character - '0' + 5;

                binaryData.AppendBits(value, 5);
            }

            else if ((character >= 'A') && (character <= 'Z'))  // A-Z
            {
                value = character - (int)'A' + 0x40;
                binaryData.AppendBits(value, 7);

            }

            else if ((character >= 'a') && (character <= 'z'))  // a-z
            {
                value = character - 'a' + 0x5a;
                binaryData.AppendBits(value, 7);
            }

            else
            {
                if (character == ' ')
                    value = 0xfc; // sp

                 else if (character == '_')
                    value = 0xfb; // _

                else if (character >= ':')
                    value = (character - 58) + 0xf5; // : -> ?

                else if (character >= '%')
                    value = character - 37 + 0xea; // % -> /

                else
                    value = character - 33 + 0xe8; // ! -> "

                binaryData.AppendBits(value, 8);
            }
        }

        static void ProcessALPHMode()
        {
            int value;

            // Check next character type.
            if (char.IsUpper(inputData[position]))
            {
                // Alpha.
                value = inputData[position] - 65;
                binaryData.AppendBits(value, 5);
                position++;
            }

            else if (char.IsDigit(inputData[position]))
            {
                // Number.
                value = inputData[position] + 4;
                binaryData.AppendBits(value, 6);
                position++;
            }

            else if (inputData[position] == FNC1)
            {
                // FNC1.
                binaryData.AppendBits(0x1f, 5);
                currentMode = NUM_MODE;
                position++;
            }

            if(position == inputLength)
            {
                binaryData.AppendBits(0x1f, 5);
                currentMode = NUM_MODE;
            }
        }

        static int GetTargetSize(CompositeMode compositeMode, ref int dataColumns, ref int eccLevel, int linearWidth)
        {
            int binaryLength = binaryData.SizeInBits;
            int targetBitSize = 0;
            int index = 0;
            int rowCount = 0;
            int columns = dataColumns - 2;
            int length;

            if (compositeMode == CompositeMode.CCA)
            {
                length = CCATable[columns].Length;
                while (index < length && binaryLength <= CCATable[columns][index])
                {
                    targetBitSize = CCATable[columns][index];
                    index++;
                }
            }

            if (compositeMode == CompositeMode.CCB)
            {
                length = CCBTable[columns].Length;
                while (index < length && binaryLength <= CCBTable[columns][index])
                {
                    targetBitSize = CCBTable[columns][index];
                    index++;
                }
            }

            if (compositeMode == CompositeMode.CCC)
            {
                // CC-C 2D Component is a bit more complex!
                int byteLength = binaryData.SizeInBytes;
                int codewordsUsed = ((byteLength / 6) * 5) + (byteLength % 6);

                eccLevel = 7;
                if (codewordsUsed <= 1280)
                    eccLevel = 6;

                if (codewordsUsed <= 640)
                    eccLevel = 5;

                if (codewordsUsed <= 320)
                    eccLevel = 4;

                if (codewordsUsed <= 160)
                    eccLevel = 3;

                if (codewordsUsed <= 40)
                    eccLevel = 2;

                int eccCodewords = 1;
                for (int i = 0; i <= eccLevel; i++)
                    eccCodewords *= 2;

                codewordsUsed += eccCodewords + 3;
                dataColumns = (linearWidth - 62) / 17;
                // Stop the symbol from becoming too high.
                
                do
                {
                    dataColumns++;
                    rowCount = codewordsUsed / dataColumns;
                } while (rowCount > 90);

                if (codewordsUsed % dataColumns != 0)
                    rowCount++;

                int targetCodewords = (dataColumns * rowCount) - eccCodewords - 3;
                int targetByteSize = ((targetCodewords / 5) * 6) + (targetCodewords % 5);
                targetBitSize = 8 * targetByteSize;
            }

            return targetBitSize;
        }

        /// <summary>
        /// Adds the required padding to the binary bit stream.
        /// </summary>
        /// <param name="targetBitSize"></param>
        static void AddPadding(int targetBitSize)
        {
            if (currentMode == NUM_MODE)
                binaryData.AppendBits(0, 4);    // Latch to alphanumeric;

            while (binaryData.SizeInBits < targetBitSize)
                binaryData.AppendBits(0x04, 5);

            if (binaryData.SizeInBits > targetBitSize)
                binaryData.RemoveBits(targetBitSize, binaryData.SizeInBits - targetBitSize);
        }

        /// <summary>
        /// Encodes 13 digits into 44 bits.
        /// </summary>
        static void Convert13()
        {
            int value = inputData[position] - '0';
            binaryData.AppendBits(value, 4);
            position++;
            Convert12();
        }

        /// <summary>
        /// Encodes 12 digits into 40 bits.
        /// </summary>
        static void Convert12()
        {
            for (int i = 0; i < 4 ; i++)
            {
                int value = (((inputData[position] - '0') * 100) + ((inputData[position + 1] - '0') * 10) + inputData[position + 2] - '0');
                binaryData.AppendBits(value, 10);
                position += 3;
            }
        }

        /// <summary>
        /// Encodes a date into 16 bits.
        /// </summary>
        static void ConvertDate()
        {
            int value = (((inputData[position] - '0') * 10) + (inputData[position + 1] - '0')) * 384;       // YY
            value += (((inputData[position + 2] - '0') * 10) + ((inputData[position + 3] - '0') - 1)) * 32; // MM
            value += (((inputData[position + 4] - '0') * 10) + (inputData[position + 5] - '0'));            // DD
            binaryData.AppendBits(value, 16);
            position += 6;
        }
    }
}