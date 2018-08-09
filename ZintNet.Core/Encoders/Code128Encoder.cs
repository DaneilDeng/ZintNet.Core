/* Code128Encoder.cs - Handles Code 128 based 1D symbols */

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
    internal class Code128Encoder : SymbolEncoder
    {
        #region Tables and Constants
        private static string[] Code128Table = {
		    "212222","222122","222221","121223","121322","131222","122213","122312","132212",
		    "221213","221312","231212","112232","122132","122231","113222","123122","123221",
		    "223211","221132","221231","213212","223112","312131","311222","321122","321221",
		    "312212","322112","322211","212123","212321","232121","111323","131123","131321",
		    "112313","142113","132311","211313","231113","231311","112133","112331","132131",
		    "113123","113321","133121","313121","211331","231131","213113","213311","213131",
		    "311123","311321","331121","312113","312311","332111","324111","221411","431111",
		    "111224","111422","121124","121421","141122","141221","112214","112412","122114",
		    "122411","142112","142211","241211","221114","413111","241112","134111","111242",
		    "121142","121241","114212","124112","124211","411212","421112","421211","212141",
		    "214121","412121","111143","111341","131141","114113","114311","411112","411311",
		    "113141","114131","311141","411131","211412","211214","211232","2331112" };

        private const int SHIFTA = 90;
        private const int LATCHA = 91;
        private const int SHIFTB = 92;
        private const int LATCHB = 93;
        private const int SHIFTC = 94;
        private const int LATCHC = 95;
        private const int AORB = 96;
        private const int ABORC = 97;
        #endregion

        private StringBuilder rowPattern;
        private int[,] encodingList = new int[2, 170];

        public Code128Encoder(Symbology symbology, string barcodeMessage, string compositeMessage, CompositeMode compositeMode, EncodingMode encodingMode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.compositeMessage = compositeMessage;
            this.isCompositeSymbol = !String.IsNullOrEmpty(compositeMessage);
            this.compositeMode = compositeMode;
            this.encodingMode = encodingMode;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.Code128:
                    switch (encodingMode)
                    {
                        case EncodingMode.Standard:
                            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                            Code128();
                            barcodeText = new String(barcodeData);
                            break;

                        case EncodingMode.GS1:
                            isGS1 = true;
                            barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                            EAN128();
                            barcodeText = barcodeMessage;
                            barcodeText = barcodeText.Replace('[', '(');
                            barcodeText = barcodeText.Replace(']', ')');
                            break;

                        case EncodingMode.HIBC:
                            barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                            Code128();
                            barcodeText = new String(barcodeData);
                            break;
                    }

                    break;

                case Symbology.EAN14:
                    isCompositeSymbol = false;
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    EAN14();
                    barcodeText = new string(barcodeData);
                    break;

                case Symbology.SSCC18:
                    isCompositeSymbol = false;
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    SSC18();
                    barcodeText = new string(barcodeData);
                    break;
            }

            // Expand the row pattern into the symbol data.
            SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 0.0f);

            // Build the symbol separator and add the 2D component (GS1-128)
            if (isGS1 && isCompositeSymbol)
            {
                byte[] rowData = new byte[Symbol[0].GetRowData().Length];
                for (int i = 0; i < rowData.Length; i++)
                    if (Symbol[0].GetRowData()[i] == 0)
                        rowData[i] = 1;

                // Insert the separator to the symbol (top down ).
                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(0, symbolData);
                CompositeEncoder.AddComposite(symbolId, compositeMessage, Symbol, compositeMode, rowData.Length);
            }

            return Symbol;
        }

        private void SSC18()
        {
            int inputLength = barcodeData.Length;
            char checkDigit;

            if (inputLength > 17)
                throw new InvalidDataLengthException("SSC18: Too many characters in the input data.");

            for (int i = 0; i < inputLength; i++)
            {
                if (!Char.IsDigit(barcodeData[i]))
                    throw new InvalidDataException("SSC18: Only numeric data allowed.");
            }

            if (inputLength < 17)
            {
                string zeros = new String('0', 17 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
                inputLength = barcodeData.Length;
            }

            checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
            barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
            barcodeData = ArrayExtensions.Insert(barcodeData, 0, "(00)");
            EAN128();
        }

        private void EAN14()
        {
            int inputLength = barcodeData.Length;
            char checkDigit;

            if (inputLength > 13)
                throw new InvalidDataLengthException("EAN14: Too many characters in the input data.");

            for (int i = 0; i < inputLength; i++)
            {
                if (!Char.IsDigit(barcodeData[i]))
                    throw new InvalidDataException("EAN14: Only numeric data allowed.");
            }

            if (inputLength < 13)
            {
                string zeros = new String('0', 13 - inputLength);
                barcodeData = ArrayExtensions.Insert(barcodeData, 0, zeros);
                inputLength = barcodeData.Length;
            }

            checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
            barcodeData = ArrayExtensions.Insert(barcodeData, inputLength, checkDigit);
            barcodeData = ArrayExtensions.Insert(barcodeData, 0, "(01)");
            EAN128();
        }

        // Handle Code 128 standard.
        private void Code128()
        {
            int position, totalSum;
            int j = 0;
            int[] values = new int[170];
            int barCharacters = 0;
            int fState = 0;
            int mode;
            char lastSet, currentSet = ' ';
            char[] set = new char[170];
            char[] fset = new char[170];
            float glyphCount = 0.0f;
            int inputLength = barcodeData.Length;
            bool code128B = false;
            rowPattern = new StringBuilder();

            if (encodingMode == EncodingMode.HIBC)
                code128B = true;    // Encode HIBC in Code128B.

            if (inputLength > 160)
            {
                // This only blocks rediculously long input - the actual length of the
                // resulting barcode depends on the type of data, so this is trapped later.
                throw new InvalidDataLengthException("Code 128: Input data too long.");
            }

            // Initialise set and fset to spaces.
            for (int i = 0; i < set.Length; i++)
            {
                set[i] = ' ';
                fset[i] = ' ';
            }

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

            // Decide on mode using rules of ISO 15417 Annex E.
            int listIndex = 0;
            int sourceIndex = 0;
            for (int i = 0; i < 170; i++)
                encodingList[0, i] = 0;

            mode = GetMode(barcodeData[sourceIndex]);
            if (code128B && (mode == ABORC))
                mode = AORB;

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
                    if (code128B && (mode == ABORC))
                        mode = AORB;
                }

                listIndex++;
            } while (sourceIndex < inputLength);

            DxSmooth(ref listIndex);

            // Resolve odd length LATCHC blocks.
            if ((encodingList[1, 0] == LATCHC) && (encodingList[0, 0] & 1) > 0)
            {
                // Rule 2.
                encodingList[0, 1]++;
                encodingList[0, 0]--;
                if (listIndex == 1)
                {
                    encodingList[0, 1] = 1;
                    encodingList[1, 1] = LATCHB;
                    listIndex = 2;
                }
            }

            if (listIndex > 1)
            {
                for (int i = 1; i < listIndex; i++)
                {
                    if ((encodingList[1, i] == LATCHC) && (encodingList[0, i] & 1) > 0)
                    {
                        // Rule 3b.
                        encodingList[0, i - 1]++;
                        encodingList[0, i]--;
                    }
                }
            }

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

            // Now we can calculate how long the barcode is going to be - and stop it from being too long.
            lastSet = ' ';

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

                if (i == 0)
                {
                    if (fset[i] == 'F')
                        glyphCount = glyphCount + 2.0f;

                }

                else
                {
                    if ((fset[i] == 'F') && (fset[i - 1] != 'F'))
                        glyphCount = glyphCount + 2.0f;

                    if ((fset[i] != 'F') && (fset[i - 1] == 'F'))
                        glyphCount = glyphCount + 2.0f;
                }

                if (set[i] == 'C')
                    glyphCount = glyphCount + 0.5f;

                else
                    glyphCount = glyphCount + 1.0f;
            }

            if (glyphCount > 80.0)
                throw new InvalidDataLengthException("Code 128: Input too long.");

            // So now we know what start character to use - we can get on with it!
            /*if (symbol->output_options & READER_INIT)
            {
                // Reader Initialisation mode.
                switch (set[0])
                {
                    case 'A': // Start A.
                        binaryString.Append(Code128Table[103]);
                        //strcat(dest, C128Table[103]);
                        values[0] = 103;
                        current_set = 'A';
                        binaryString.Append(Code128Table[96]);  // FNC3.
                        //strcat(dest, C128Table[96]);
                        values[1] = 96;
                        bar_characters++;
                        break;

                    case 'B': // Start B.
                        binaryString.Append(Code128Table[104]);
                        //strcat(dest, C128Table[104]);
                        values[0] = 104;
                        current_set = 'B';
                        binaryString.Append(Code128Table[96]);  // FNC3.
                        //strcat(dest, C128Table[96]);
                        values[1] = 96;
                        bar_characters++;
                        break;

                    case 'C': // Start C.
                        binaryString.Append(Code128Table[104]); // Start B.
                        //strcat(dest, C128Table[104]); 
                        values[0] = 105;
                        binaryString.Append(Code128Table[96]);  //FNC3.
                        //strcat(dest, C128Table[96]); 
                        values[1] = 96;
                        binaryString.Append(Code128Table[99]);  // Code C.
                        //strcat(dest, C128Table[99]);
                        values[2] = 99;
                        bar_characters += 2;
                        current_set = 'C';
                        break;
                }
            }*/

            // else
            // {
            // Normal mode.
            switch (set[0])
            {
                case 'A': // Start A.
                    rowPattern.Append(Code128Table[103]);
                    values[0] = 103;
                    currentSet = 'A';
                    break;

                case 'B': // Start B.
                    rowPattern.Append(Code128Table[104]);
                    values[0] = 104;
                    currentSet = 'B';
                    break;

                case 'C': // Start C.
                    rowPattern.Append(Code128Table[105]);
                    values[0] = 105;
                    currentSet = 'C';
                    break;
            }
            // }

            barCharacters++;
            lastSet = set[0];

            if (fset[0] == 'F')
            {
                switch (currentSet)
                {
                    case 'A':
                        rowPattern.Append(Code128Table[101]);
                        rowPattern.Append(Code128Table[101]);
                        values[barCharacters] = 101;
                        values[barCharacters + 1] = 101;
                        break;

                    case 'B':
                        rowPattern.Append(Code128Table[100]);
                        rowPattern.Append(Code128Table[100]);
                        values[barCharacters] = 100;
                        values[barCharacters + 1] = 100;
                        break;
                }

                barCharacters += 2;
                fState = 1;
            }

            // Encode the data.
            position = 0;
            do
            {
                if ((position != 0) && (set[position] != currentSet))
                {
                    // Latch different code set.
                    switch (set[position])
                    {
                        case 'A':
                            rowPattern.Append(Code128Table[101]);
                            values[barCharacters] = 101;
                            barCharacters++;
                            currentSet = 'A';
                            break;

                        case 'B':
                            rowPattern.Append(Code128Table[100]);
                            values[barCharacters] = 100;
                            barCharacters++;
                            currentSet = 'B';
                            break;

                        case 'C':
                            rowPattern.Append(Code128Table[99]);
                            values[barCharacters] = 99;
                            barCharacters++;
                            currentSet = 'C';
                            break;
                    }
                }

                if (position != 0)
                {
                    if ((fset[position] == 'F') && (fState == 0))
                    {
                        // Latch beginning of extended mode.
                        switch (currentSet)
                        {
                            case 'A':
                                rowPattern.Append(Code128Table[101]);
                                rowPattern.Append(Code128Table[101]);
                                values[barCharacters] = 101;
                                values[barCharacters + 1] = 101;
                                break;

                            case 'B':
                                rowPattern.Append(Code128Table[100]);
                                rowPattern.Append(Code128Table[100]);
                                values[barCharacters] = 100;
                                values[barCharacters + 1] = 100;
                                break;
                        }

                        barCharacters += 2;
                        fState = 1;
                    }

                    if ((fset[position] == ' ') && (fState == 1))
                    {
                        // Latch end of extended mode.
                        switch (currentSet)
                        {
                            case 'A':
                                rowPattern.Append(Code128Table[101]);
                                rowPattern.Append(Code128Table[101]);
                                values[barCharacters] = 101;
                                values[barCharacters + 1] = 101;
                                break;

                            case 'B':
                                rowPattern.Append(Code128Table[100]);
                                rowPattern.Append(Code128Table[100]);
                                values[barCharacters] = 100;
                                values[barCharacters + 1] = 100;
                                break;
                        }

                        barCharacters += 2;
                        fState = 0;
                    }
                }

                if ((fset[position] == 'f') || (fset[position] == 'n'))
                {
                    // Shift to or from extended mode.
                    switch (currentSet)
                    {
                        case 'A':
                            rowPattern.Append(Code128Table[101]); // FNC4.
                            values[barCharacters] = 101;
                            break;

                        case 'B':
                            rowPattern.Append(Code128Table[100]); //FNC4.
                            values[barCharacters] = 100;
                            break;
                    }

                    barCharacters++;
                }

                if ((set[position] == 'a') || (set[position] == 'b'))
                {
                    // Insert shift character.
                    rowPattern.Append(Code128Table[98]);
                    values[barCharacters] = 98;
                    barCharacters++;
                }

                switch (set[position])
                {
                    // Encode data characters.
                    case 'a':
                    case 'A':
                        C128SetA(barcodeData[position], values, ref barCharacters);
                        position++;
                        break;

                    case 'b':
                    case 'B':
                        C128SetB(barcodeData[position], values, ref barCharacters);
                        position++;
                        break;

                    case 'C':
                        C128SetC(barcodeData[position], barcodeData[position + 1], values, ref barCharacters);
                        position += 2;
                        break;
                }

            } while (position < inputLength);

            // Check digit calculation
            totalSum = 0;

            for (int i = 0; i < barCharacters; i++)
            {
                if (i > 0)
                    values[i] *= i;

                totalSum += values[i];
            }

            rowPattern.Append(Code128Table[totalSum % 103]);

            // Stop character.
            rowPattern.Append(Code128Table[106]);
        }

        // Handle EAN-128 (Now known as GS1-128)
        private void EAN128()
        {
            int j = 0;
            int barCharacters = 0;
            uint position, totalSum;
            int[] values = new int[170];
            char[] set = new char[170];
            int mode, lastSet;
            float glyphCount;

            int inputLength = barcodeData.Length;
            rowPattern = new StringBuilder();

            if (inputLength > 160)
                // This only blocks rediculously long input - the actual length of the
                // resulting barcode depends on the type of data, so this is trapped later.
                throw new InvalidDataLengthException("Code GS1 128: Input data too long.");

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] == '\0')
                    // Null characters not allowed!
                    throw new InvalidDataException("Code GS1-128: Null character not permitted.");
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
                    if (barcodeData[sourceIndex] == '[')
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

            if (glyphCount > 80.0)
                throw new InvalidDataLengthException("Code GS1 128: Input data too long.");

            // So now we know what start character to use - we can get on with it!
            switch (set[0])
            {
                case 'A': /* Start A */
                    rowPattern.Append(Code128Table[103]);
                    values[0] = 103;
                    break;

                case 'B': /* Start B */
                    rowPattern.Append(Code128Table[104]);
                    values[0] = 104;
                    break;

                case 'C': /* Start C */
                    rowPattern.Append(Code128Table[105]);
                    values[0] = 105;
                    break;
            }

            barCharacters++;
            rowPattern.Append(Code128Table[102]);
            values[1] = 102;
            barCharacters++;

            // Encode the data.
            position = 0;
            do
            {

                if ((position != 0) && (set[position] != set[position - 1]))
                {
                    // Latch different code set.
                    switch (set[position])
                    {
                        case 'A':
                            rowPattern.Append(Code128Table[101]);
                            values[barCharacters] = 101;
                            barCharacters++;
                            break;

                        case 'B':
                            rowPattern.Append(Code128Table[100]);
                            values[barCharacters] = 100;
                            barCharacters++;
                            break;

                        case 'C':
                            rowPattern.Append(Code128Table[99]);
                            values[barCharacters] = 99;
                            barCharacters++;
                            break;
                    }
                }

                if ((set[position] == 'a') || (set[position] == 'b'))
                {
                    // Insert shift character.
                    rowPattern.Append(Code128Table[98]);
                    values[barCharacters] = 98;
                    barCharacters++;
                }

                if (barcodeData[position] != '[')
                {
                    switch (set[position])
                    {
                        // Encode data characters.
                        case 'A':
                        case 'a':
                            C128SetA(barcodeData[position], values, ref barCharacters);
                            position++;
                            break;

                        case 'B':
                        case 'b':
                            C128SetB(barcodeData[position], values, ref barCharacters);
                            position++;
                            break;

                        case 'C':
                            C128SetC(barcodeData[position], barcodeData[position + 1], values, ref barCharacters);
                            position += 2;
                            break;
                    }
                }

                else
                {
                    rowPattern.Append(Code128Table[102]);
                    values[barCharacters] = 102;
                    barCharacters++;
                    position++;
                }
            } while (position < inputLength);

            // "...note that the linkage flag is an extra code set character between
            // the last data character and the Symbol Check Character" (GS1 Specification)
            // Linkage flags in GS1-128 are determined by ISO/IEC 24723 section 7.4

            if (isCompositeSymbol)
            {
                int linkValue = 0;
                switch (compositeMode)
                {
                    case CompositeMode.CCA:
                    case CompositeMode.CCB:
                        // CC-A or CC-B 2D component.
                        switch (set[inputLength - 1])
                        {
                            case 'A': linkValue = 100; break;
                            case 'B': linkValue = 99; break;
                            case 'C': linkValue = 101; break;
                        }

                        break;

                    case CompositeMode.CCC:
                        // CC-C 2D component.
                        switch (set[inputLength - 1])
                        {
                            case 'A': linkValue = 99; break;
                            case 'B': linkValue = 101; break;
                            case 'C': linkValue = 100; break;
                        }

                        break;
                }


                if (linkValue != 0)
                {
                    rowPattern.Append(Code128Table[linkValue]);
                    values[barCharacters] = linkValue;
                    barCharacters++;
                }
            }

            // Check digit calculation
            totalSum = 0;
            for (int i = 0; i < barCharacters; i++)
            {
                if (i > 0)
                    values[i] *= i;

                totalSum += (uint)values[i];
            }

            rowPattern.Append(Code128Table[totalSum % 103]);

            // Stop character.
            rowPattern.Append(Code128Table[106]);
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

                    if ((listIndex == 1) && ((length == 2) && (current == ABORC)))    // Rule 1a. //*(list)
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

                    if ((current == SHIFTA) && (length > 1))
                    {
                        /* Rule 4 */
                        encodingList[1, i] = LATCHA;
                        current = LATCHA;
                    }

                    if ((current == SHIFTB) && (length > 1))
                    {
                        /* Rule 5 */
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

        // Translate Code 128 Set A characters into barcodes.
        // This set handles all control characters NULL to US.
        private void C128SetA(char source, int[] values, ref int barCharacters)
        {
            if (source > 127)
            {
                if (source < 160)
                {
                    rowPattern.Append(Code128Table[(source - 128) + 64]);
                    values[barCharacters] = (source - 128) + 64;
                }

                else
                {
                    rowPattern.Append(Code128Table[(source - 128) - 32]);
                    values[barCharacters] = (source - 128) - 32;
                }
            }

            else
            {
                if (source < 32)
                {
                    rowPattern.Append(Code128Table[source + 64]);
                    values[barCharacters] = source + 64;
                }

                else
                {
                    rowPattern.Append(Code128Table[source - 32]);
                    values[barCharacters] = source - 32;
                }
            }

            barCharacters++;
        }

        /**
         * Translate Code 128 Set B characters into barcodes.
         * This set handles all characters which are not part of long numbers and not
         * control characters.
         */
        private void C128SetB(char source, int[] values, ref int barCharacters)
        {
            if (source > 127)
            {
                rowPattern.Append(Code128Table[source - 32 - 128]);
                values[barCharacters] = source - 32 - 128;
            }

            else
            {
                rowPattern.Append(Code128Table[source - 32]);
                values[barCharacters] = source - 32;
            }

            barCharacters++;
        }

        /* Translate Code 128 Set C characters into barcodes
         * This set handles numbers in a compressed form
         */
        private void C128SetC(char sourceA, char sourceB, int[] values, ref int barCharacters)
        {
            int weight = (10 * (sourceA - '0')) + (sourceB - '0');
            rowPattern.Append(Code128Table[weight]);
            values[barCharacters] = weight;
            barCharacters++;
        }
    }
}
