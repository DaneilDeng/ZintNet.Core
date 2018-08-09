/* Code16Encoder.cs - Handles Code16K 2D symbol */

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
using System.Linq;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class Code16KEncoder : SymbolEncoder
    {
        #region Tables and Constants
        private static string[] Code16KTable = {
		    "212222", "222122", "222221", "121223", "121322", "131222", "122213",
            "122312", "132212", "221213", "221312", "231212", "112232", "122132", "122231", "113222",
            "123122", "123221", "223211", "221132", "221231", "213212", "223112", "312131", "311222",
            "321122", "321221", "312212", "322112", "322211", "212123", "212321", "232121", "111323",
            "131123", "131321", "112313", "132113", "132311", "211313", "231113", "231311", "112133",
            "112331", "132131", "113123", "113321", "133121", "313121", "211331", "231131", "213113",
            "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111", "314111",
            "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
            "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112",
            "134111", "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112",
            "421211", "212141", "214121", "412121", "111143", "111341", "131141", "114113", "114311",
            "411113", "411311", "113141", "114131", "311141", "411131", "211412", "211214", "211232",
            "211133" };

        // EN 12323 Table 3 and Table 4 - Start patterns and stop patterns.
        private static string[] C16KStartStop = { "3211", "2221", "2122", "1411", "1132", "1231", "1114", "3112" };


        // EN 12323 Table 5 - Start and stop values defining row numbers.
        private static int[] C16KStartValues = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7 };
        private static int[] C16KStopValues = {  0, 1, 2, 3, 4, 5, 6, 7, 4, 5, 6, 7, 0, 1, 2, 3 };

        private const int SHIFTA = 90;
        private const int LATCHA = 91;
        private const int SHIFTB = 92;
        private const int LATCHB = 93;
        private const int SHIFTC = 94;
        private const int LATCHC = 95;
        private const int AORB = 96;
        private const int ABORC = 97;
        #endregion

        private int[,] encodingList = new int[2, 170];

        public Code16KEncoder(Symbology symbology, string barcodeMessage, EncodingMode encodingMode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.encodingMode = encodingMode;
            this.elementsPerCharacter = 11;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (encodingMode)
            {
                case EncodingMode.Standard:
                    isGS1 = false;
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    Code16K();
                    break;

                case EncodingMode.GS1:
                    isGS1 = true;
                    barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                    Code16K();
                    break;
            }

            return Symbol;
        }

        private void Code16K()
        {
            int rowsRequired;
            int padding;
            bool fState;
            int mode, lastSet, currentSet;
            char[] set = new char[160];
            char[] fset = new char[160];
            int m, position;
            int[] values = new int[160];
            int barCharacters;
            float glyphCount;
            int glyphs;
            int checkValue1, checkValue2;
            int checkSum1, checkSum2;
            int inputLength;
            int j = 0;
            StringBuilder rowPattern;
            inputLength = barcodeData.Length;
            if (inputLength > 157)
                throw new InvalidDataLengthException("Code 16K: Input data too long.");

            // Initialise set and fset to spaces.
            for (int i = 0; i < set.Length; i++)
            {
                set[i] = ' ';
                fset[i] = ' ';
            }

            barCharacters = 0;
            // Detect extended ASCII characters.
            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] >= 128)
                    fset[i] = 'f';
            }

            // Decide when to latch to extended mode - Annex E note 3.
            for (int i = 0; i < inputLength; i++)
            {
                if (fset[i] == 'f')
                    j++;

                else
                    j = 0;

                if (j >= 5)
                {
                    for (int k = i; k > (i - 5); k--)
                        fset[k] = 'F';
                }

                if ((j >= 3) && (i == (inputLength - 1)))
                {
                    for (int k = i; k > (i - 3); k--)
                        fset[k] = 'F';
                }
            }

            // Decide if it is worth reverting to 646 encodation for a few characters as described in 4.3.4.2 (d).
            for (int i = 1; i < inputLength; i++)
            {
                if ((fset[i - 1] == 'F') && (fset[i] == ' '))
                {
                    // Detected a change from 8859-1 to 646 - count how long for.
                    for (j = 0; (fset[i + j] == ' ') && ((i + j) < inputLength); j++) ;
                    if ((j < 5) || ((j < 3) && ((i + j) == (inputLength - 1))))
                    {
                        // Uses the same figures recommended by Annex E note 3.
                        // Change to shifting back rather than latching back.
                        for (int k = 0; k < j; k++)
                            fset[i + k] = 'n';
                    }
                }
            }

            // Decide on mode using of ISO 15417 Annex E
            int listIndex = 0;
            int sourceIndex = 0;
            mode = GetMode(barcodeData[sourceIndex]);
            if (barcodeData[sourceIndex] == '[')
                mode = ABORC;

            for (int i = 0; i < 170; i++)
                encodingList[0, i] = 0;

            do
            {
                encodingList[1, listIndex] = mode;
                while ((encodingList[1, listIndex] == mode) && (sourceIndex < inputLength))
                {
                    encodingList[0, listIndex]++;
                    sourceIndex++;
                    if (sourceIndex >= inputLength)
                        break;

                    mode = GetMode(barcodeData[sourceIndex]);
                    if (isGS1 && barcodeData[sourceIndex] == '[')
                        mode = ABORC;
                }

                listIndex++;
            } while (sourceIndex < inputLength);

            DxSmooth(ref listIndex);
            // Put set data into set[].
            position = 0;
            for (int i = 0; i < listIndex; i++)
            {
                for (j = 0; j < encodingList[0, i]; j++)
                {
                    switch (encodingList[1, i])
                    {
                        case SHIFTA:
                            set[position] = 'a';
                            break;

                        case LATCHA:
                            set[position] = 'A';
                            break;

                        case SHIFTB:
                            set[position] = 'b';
                            break;

                        case LATCHB:
                            set[position] = 'B';
                            break;

                        case LATCHC:
                            set[position] = 'C';
                            break;
                    }

                    position++;
                }
            }

            // Adjust for strings which start with shift characters - make them latch instead.
            if (set[0] == 'a')
            {
                int idx = 0;
                do
                {
                    set[idx] = 'A';
                    idx++;
                } while (set[idx] == 'a');
            }

            if (set[0] == 'b')
            {
                int idx = 0;
                do
                {
                    set[idx] = 'B';
                    idx++;
                } while (set[idx] == 'b');
            }

            // Watch out for odd-length Mode C blocks.
            int count = 0;
            int x;

            for (x = 0; x < position; x++)
            {
                if (set[x] == 'C')
                {
                    if (barcodeData[x] == '[')
                    {
                        if ((count & 1) > 0)
                        {
                            if ((x - count) != 0)
                                set[x - count] = 'B';

                            else
                                set[x - 1] = 'B';
                        }

                        count = 0;
                    }

                    else
                        count++;
                }

                else
                {
                    if ((count & 1) > 0)
                    {
                        if ((x - count) != 0)
                            set[x - count] = 'B';

                        else
                            set[x - 1] = 'B';
                    }

                    count = 0;
                }
            }

            if ((count & 1) > 0)
            {
                if ((x - count) != 0)
                    set[x - count] = 'B';

                else
                    set[x - 1] = 'B';
            }

            for (x = 1; x < position - 1; x++)
            {
                if ((set[x] == 'C') && ((set[x - 1] == 'B') && (set[x + 1] == 'B')))
                    set[x] = 'B';
            }

            // Now we can calculate how long the barcode is going to be - and stop it from being too long.
            lastSet = ' ';
            glyphCount = 0.0f;

            for (int i = 0; i < inputLength; i++)
            {
                if ((set[i] == 'a') || (set[i] == 'b'))
                    glyphCount = glyphCount + 1.0f;

                if ((fset[i] == 'f') || (fset[i] == 'n'))
                    glyphCount = glyphCount + 1.0f;

                if (((set[i] == 'A') || (set[i] == 'B')) || (set[i] == 'C'))
                {
                    if (set[i] != lastSet)
                    {
                        lastSet = set[i];
                        glyphCount = glyphCount + 1.0f;
                    }
                }

                if ((set[i] == 'C') && (barcodeData[i] != '['))
                    glyphCount = glyphCount + 0.5f;

                else
                    glyphCount = glyphCount + 1.0f;
            }

            if (isGS1 && (set[0] != 'A'))   // FNC1 can be integrated with mode character.
                glyphCount--;

            if (glyphCount > 77.0)
                throw new InvalidDataLengthException("Code 16K: Input data too long.");

            // Calculate how tall the symbol will be.
            glyphCount = glyphCount + 2.0f;
            glyphs = (int)glyphCount;
            rowsRequired = (glyphs / 5);
            if (glyphs % 5 > 0)
                rowsRequired++;

            if (rowsRequired == 1)
                rowsRequired = 2;

            // Start with the mode character - Table 2.
            m = 0;
            switch (set[0])
            {
                case 'A':
                    m = 0;
                    break;

                case 'B':
                    m = 1;
                    break;

                case 'C':
                    m = 2;
                    break;
            }

            /*if (symbol->output_options & READER_INIT)
            {
                if (m == 2)
                    m = 5;

                if (isGS1)
                {
                    strcpy(symbol->errtxt, "Cannot use both GS1 mode and Reader Initialisation (D22)");
                    return ZINT_ERROR_INVALID_OPTION;
                }
        
                else
                {
                    if ((set[0] == 'B') && (set[1] == 'C'))
                        m = 6;
                }

                values[bar_characters] = (7 * (rowsRequired - 2)) + m; // See 4.3.4.2
                values[bar_characters + 1] = 96; // FNC3.
                bar_characters += 2;
                }
    
            else
            {*/
            if (isGS1)
            {
                // Integrate FNC1.
                switch (set[0])
                {
                    case 'B':
                        m = 3;
                        break;

                    case 'C':
                        m = 4;
                        break;
                }
            }

            else
            {
                if ((set[0] == 'B') && (set[1] == 'C'))
                    m = 5;

                if (((set[0] == 'B') && (set[1] == 'B')) && (set[2] == 'C'))
                    m = 6;
            }

            values[barCharacters] = (7 * (rowsRequired - 2)) + m; // See 4.3.4.2
            barCharacters++;

            currentSet = set[0];
            fState = false;    // fState remembers if we are in Extended ASCII mode (value true) or in ISO/IEC 646 mode (value false) 
            
            if (fset[0] == 'F')
            {
                switch (currentSet)
                {
                    case 'A':
                        values[barCharacters] = 101;
                        values[barCharacters + 1] = 101;
                        break;

                    case 'B':
                        values[barCharacters] = 100;
                        values[barCharacters + 1] = 100;
                        break;

                }

                barCharacters += 2;
                fState = true;
            }

            position = 0;

            // Encode the data.
            do
            {

                if ((position != 0) && (set[position] != set[position - 1]))
                {
                    /* Latch different code set */
                    switch (set[position])
                    {
                        case 'A':
                            values[barCharacters] = 101;
                            barCharacters++;
                            currentSet = 'A';
                            break;

                        case 'B':
                            values[barCharacters] = 100;
                            barCharacters++;
                            currentSet = 'B';
                            break;

                        case 'C':
                            if (!((position == 1) && (set[0] == 'B')))
                            {
                                // Not Mode C, Shift B.
                                if (!((position == 2) && ((set[0] == 'B') && (set[1] == 'B'))))
                                {
                                    // Not Mode C, Double Shift B.
                                    values[barCharacters] = 99;
                                    barCharacters++;
                                }
                            }

                            currentSet = 'C';
                            break;
                    }
                }

                if (position != 0)
                {
                    if ((fset[position] == 'F') && (fState == false))
                    {
                        // Latch beginning of extended mode.
                        switch (currentSet)
                        {
                            case 'A':
                                values[barCharacters] = 101;
                                values[barCharacters + 1] = 101;
                                break;

                            case 'B':
                                values[barCharacters] = 100;
                                values[barCharacters + 1] = 100;
                                break;
                        }

                        barCharacters += 2;
                        fState = true;
                    }

                    if ((fset[position] == ' ') && (fState == true))
                    {
                        // Latch end of extended mode.
                        switch (currentSet)
                        {
                            case 'A':
                                values[barCharacters] = 101;
                                values[barCharacters + 1] = 101;
                                break;

                            case 'B':
                                values[barCharacters] = 100;
                                values[barCharacters + 1] = 100;
                                break;
                        }

                        barCharacters += 2;
                        fState = false;
                    }
                }

                if ((fset[glyphs] == 'f') || (fset[glyphs] == 'n'))
                {
                    // Shift extended mode.
                    switch (currentSet)
                    {
                        case 'A':
                            values[barCharacters] = 101; /* FNC 4 */
                            break;

                        case 'B':
                            values[barCharacters] = 100; /* FNC 4 */
                            break;
                    }

                    barCharacters++;
                }

                if ((set[glyphs] == 'a') || (set[glyphs] == 'b'))
                {
                    // Insert shift character.
                    values[barCharacters] = 98;
                    barCharacters++;
                }

                if (!((isGS1) && (barcodeData[position] == '[')))
                {
                    switch (set[position])
                    { /* Encode data characters */
                        case 'A':
                        case 'a':
                            Code16KSetA(barcodeData[position], values, ref barCharacters);
                            position++;
                            break;

                        case 'B':
                        case 'b':
                            Code16KSetB(barcodeData[position], values, ref barCharacters);
                            position++;
                            break;

                        case 'C': Code16KSetC(barcodeData[position], barcodeData[position + 1], values, ref barCharacters);
                            position += 2;
                            break;
                    }
                }

                else
                {
                    values[barCharacters] = 102;
                    barCharacters++;
                    position++;
                }
            } while (position < inputLength);

            padding = 5 - ((barCharacters + 2) % 5);
            if (padding == 5)
                padding = 0;

            if ((barCharacters + padding) < 8)
                padding += 8 - (barCharacters + padding);

            for (int i = 0; i < padding; i++)
            {
                values[barCharacters] = 106;
                barCharacters++;
            }

            // Calculate check digits.
            checkSum1 = 0;
            checkSum2 = 0;
            for (int i = 0; i < barCharacters; i++)
            {
                checkSum1 += (i + 2) * values[i];
                checkSum2 += (i + 1) * values[i];
            }

            checkValue1 = checkSum1 % 107;
            checkSum2 += checkValue1 * (barCharacters + 1);
            checkValue2 = checkSum2 % 107;
            values[barCharacters] = checkValue1;
            values[barCharacters + 1] = checkValue2;
            barCharacters += 2;

            for (int row = 0; row < rowsRequired; row++)
            {
                rowPattern = new StringBuilder();
                rowPattern.Append(C16KStartStop[C16KStartValues[row]]);
                rowPattern.Append("1");
                for (int i = 0; i < 5; i++)
                    rowPattern.Append(Code16KTable[values[(row * 5) + i]]);

                rowPattern.Append(C16KStartStop[C16KStopValues[row]]);

                // Expand row into the symbol data.
                SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 10.0f);
            }
        }

        // Determine appropriate mode for a given character.
        private static int GetMode(char value)
        {
            int mode = 0;

            if (value <= 31)
                mode = SHIFTA;

            else if ((value >= 48) && (value <= 57))
                mode = ABORC;

            else if (value <= 95)
                mode = AORB;

            else if (value <= 127)
                mode = SHIFTB;

            else if (value <= 159)
                mode = SHIFTA;

            else if (value <= 223)
                mode = AORB;

            else
                mode = SHIFTB;

            return mode;
        }

        // Implements rules from ISO 15417 Annex E
        private void DxSmooth(ref int listIndex)
        {
            int current, last, next, length;

            for (int i = 0; i < listIndex; i++)
            {
                current = encodingList[1, i];
                length = encodingList[0, i];

                if (i != 0)
                    last = encodingList[1, i - 1];

                else
                    last = 0;

                if (i != encodingList.Length - 1)
                    next = encodingList[1, i + 1];

                else
                    next = 0;

                if (i == 0) // First block.
                {

                    if ((listIndex == 1) && ((length == 2) && (current == ABORC)))    // Rule 1a.
                        encodingList[1, i] = LATCHC;


                    if (current == ABORC)
                    {
                        if (length >= 4)    // Rule 1b.
                            encodingList[1, i] = LATCHC;

                        else
                        {
                            encodingList[1, i] = AORB;
                            current = AORB;
                        }
                    }

                    if (current == SHIFTA)  // Rule 1c.
                        encodingList[1, i] = LATCHA;

                    if ((current == AORB) && (next == SHIFTA))  // Rule 1c.
                    {
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if (current == AORB)    // Rule 1d.
                        encodingList[1, i] = LATCHB;
                }

                else
                {
                    if ((current == ABORC) && (length >= 4))    // Rule 3.
                    {
                        encodingList[1, i] = LATCHC;
                        current = LATCHC;
                    }

                    if (current == ABORC)
                    {
                        encodingList[1, i] = AORB;
                        current = AORB;
                    }

                    if ((current == AORB) && (last == LATCHA))
                    {
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == AORB) && (last == LATCHB))
                    {
                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }

                    if ((current == AORB) && (next == SHIFTA))
                    {
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == AORB) && (next == SHIFTB))
                    {
                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }

                    if (current == AORB)
                    {
                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }

                    if ((current == SHIFTA) && (length > 1))    // Rule 4.
                    {

                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == SHIFTB) && (length > 1))    // Rule 5
                    {

                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }

                    if ((current == SHIFTA) && (last == LATCHA))
                    {
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == SHIFTB) && (last == LATCHB))
                    {
                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }

                    if ((current == SHIFTA) && (last == LATCHC))
                    {
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == SHIFTB) && (last == LATCHC))
                    {
                        encodingList[1, i] = LATCHB;
                        current = LATCHB;
                    }
                } // Rule 2 is implimented elsewhere, Rule 6 is implied.
            }

            GroupBlocks(ref listIndex);
        }

        /// <summary>
        /// Brings together the same size block.
        /// </summary>
        /// <param name="listIndex">the list index</param>
        private void GroupBlocks(ref int listIndex)
        {
            int i, j;

            // Bring together same type blocks.
            if (listIndex > 1)
            {
                i = 1;
                while (i < listIndex)
                {
                    if (encodingList[1, i - 1] == encodingList[1, i])
                    {
                        // Bring together.
                        encodingList[0, i - 1] = encodingList[0, i - 1] + encodingList[0, i];
                        j = i + 1;

                        // Decreace the list.
                        while (j < listIndex)
                        {
                            encodingList[0, j - 1] = encodingList[0, j];
                            encodingList[1, j - 1] = encodingList[1, j];
                            j++;
                        }

                        listIndex -= 1;
                        i--;
                    }
                    i++;
                }
            }
        }

        private static void Code16KSetA(char source, int[] values, ref int barCharacters)
        {
            if (source > 127)
            {
                if (source < 160)
                    values[barCharacters] = (source + 64) - 128;

                else
                    values[barCharacters] = (source - 32) - 128;
            }

            else
            {
                if (source < 32)
                    values[barCharacters] = source + 64;

                else
                    values[barCharacters] = source - 32;
            }

            barCharacters++;
        }

        /**
         * Translate Code 128 Set B characters into barcodes.
         * This set handles all characters which are not part of long numbers and not
         * control characters.
         */
        private static void Code16KSetB(char source, int[] values, ref int barCharacters)
        {
            if (source > 127)
                values[barCharacters] = source - 32 - 128;

            else
                values[barCharacters] = source - 32;

            barCharacters++;
        }

        /* Translate Code 128 Set C characters into barcodes
         * This set handles numbers in a compressed form
         */
        private static void Code16KSetC(char sourceA, char sourceB, int[] values, ref int barCharacters)
        {
            int weight = (10 * (sourceA - '0')) + (sourceB - '0');
            values[barCharacters] = weight;
            barCharacters++;
        }
    }
}
