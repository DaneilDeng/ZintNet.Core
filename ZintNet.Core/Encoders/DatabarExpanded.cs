/* DatabarExpanded.cs Handles the Databar Expanded 1D + composite 2D symbol */

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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal partial class DatabarEncoder
    {
        # region Tables
        // RSS Expanded Tables.
        static int[] GroupSumExpanded = { 0, 348, 1388, 2948, 3988 };
        static int[] EvenExpanded = { 4, 20, 52, 104, 204 };
        static int[] OddModulesExpanded = { 12, 10, 8, 6, 4 };
        static int[] EvenModulesExpanded = { 5, 7, 9, 11, 13 };
        static int[] WidestOddExpanded = { 7, 5, 4, 3, 1 };
        static int[] WidestEvenExpanded = { 2, 4, 5, 6, 8 };
        static int[] ChecksumWeightExpanded = {
            // Table 14.
	        1, 3, 9, 27, 81, 32, 96, 77,
	        20, 60, 180, 118, 143, 7, 21, 63,
	        189, 145, 13, 39, 117, 140, 209, 205,
	        193, 157, 49, 147, 19, 57, 171, 91,
	        62, 186, 136, 197, 169, 85, 44, 132,
	        185, 133, 188, 142, 4, 12, 36, 108,
	        113, 128, 173, 97, 80, 29, 87, 50,
	        150, 28, 84, 41, 123, 158, 52, 156,
	        46, 138, 203, 187, 139, 206, 196, 166,
	        76, 17, 51, 153, 37, 111, 122, 155,
	        43, 129, 176, 106, 107, 110, 119, 146,
	        16, 48, 144, 10, 30, 90, 59, 177,
	        109, 116, 137, 200, 178, 112, 125, 164,
	        70, 210, 208, 202, 184, 130, 179, 115,
	        134, 191, 151, 31, 93, 68, 204, 190,
	        148, 22, 66, 198, 172, 94, 71, 2,
	        6, 18, 54, 162, 64, 192, 154, 40,
	        120, 149, 25, 75, 14, 42, 126, 167,
	        79, 26, 78, 23, 69, 207, 199, 175,
	        103, 98, 83, 38, 114, 131, 182, 124,
	        161, 61, 183, 127, 170, 88, 53, 159,
	        55, 165, 73, 8, 24, 72, 5, 15,
	        45, 135, 194, 160, 58, 174, 100, 89};

        static int[] FinderPatternExpanded = {
            // Table 15.
	        1, 8, 4, 1, 1,
	        1, 1, 4, 8, 1,
	        3, 6, 4, 1, 1,
	        1, 1, 4, 6, 3,
	        3, 4, 6, 1, 1,
	        1, 1, 6, 4, 3,
	        3, 2, 8, 1, 1,
	        1, 1, 8, 2, 3,
	        2, 6, 5, 1, 1,
	        1, 1, 5, 6, 2,
	        2, 2, 9, 1, 1,
	        1, 1, 9, 2, 2};

        static int[] FinderSequence = {
            // Table 16.
	        1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        1, 4, 3, 0, 0, 0, 0, 0, 0, 0, 0,
	        1, 6, 3, 8, 0, 0, 0, 0, 0, 0, 0,
	        1, 10, 3, 8, 5, 0, 0, 0, 0, 0, 0,
	        1, 10, 3, 8, 7, 12, 0, 0, 0, 0, 0,
	        1, 10, 3, 8, 9, 12, 11, 0, 0, 0, 0,
	        1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0,
	        1, 2, 3, 4, 5, 6, 7, 10, 9, 0, 0,
	        1, 2, 3, 4, 5, 6, 7, 10, 11, 12, 0,
	        1, 2, 3, 4, 5, 8, 7, 10, 9, 12, 11};

        static int[] RowWeights = {
	        0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 5, 6, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 9, 10, 3, 4, 13, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 17, 18, 3, 4, 13, 14, 7, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 17, 18, 3, 4, 13, 14, 11, 12, 21, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 17, 18, 3, 4, 13, 14, 15, 16, 21, 22, 19, 20, 0, 0, 0, 0, 0, 0, 0, 0,
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 0, 0, 0, 0, 0, 0,
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 17, 18, 15, 16, 0, 0, 0, 0,
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 17, 18, 19, 20, 21, 22, 0, 0,
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 13, 14, 11, 12, 17, 18, 15, 16, 21, 22, 19, 20};

        # endregion

        /// <summary>
        /// Encodes the Databar Expanded symbol.
        /// </summary>
        private void DatabarExpanded()
        {
            BitVector binaryData;
            int[] checkWidths = new int[8];

            if (segments == 0)   // Auto.
                segments = 4;
            // There shall be a minimum of 4 symbol segments (2 columns) in the
            // first row of an RSS Expanded Stacked symbol when it is the linear
            // component of a Composite symbol.
            if (isCompositeSymbol && segments < 4)
                segments = 4;

            binaryData = BitStreamEncoder.DatabarExpBitStream(symbolId, barcodeData, isCompositeSymbol, segments);
            int dataCharacters = binaryData.SizeInBits / 12;

            int[] vs = new int[dataCharacters];
            int[] group = new int[dataCharacters];
            int[] vOdd = new int[dataCharacters];
            int[] vEven = new int[dataCharacters];
            int[,] charWidths = new int[dataCharacters, 8];

            for (int i = 0; i < dataCharacters; i++)
            {
                int j = 0;
                int mask = 0x800;

                while (mask > 0)
                {
                    if (binaryData[(i * 12) + j] == 1)
                        vs[i] += mask;

                    mask >>= 1;
                    j++;
                }
            }

            for (int i = 0; i < dataCharacters; i++)
            {
                if (vs[i] <= 347)
                    group[i] = 1;

                if (vs[i] >= 348 && vs[i] <= 1387)
                    group[i] = 2;

                if (vs[i] >= 1388 && vs[i] <= 2947)
                    group[i] = 3;

                if (vs[i] >= 2948 && vs[i] <= 3987)
                    group[i] = 4;

                if (vs[i] >= 3988)
                    group[i] = 5;

                vOdd[i] = (vs[i] - GroupSumExpanded[group[i] - 1]) / EvenExpanded[group[i] - 1];
                vEven[i] = (vs[i] - GroupSumExpanded[group[i] - 1]) % EvenExpanded[group[i] - 1];

                GetRSSWidths(vOdd[i], OddModulesExpanded[group[i] - 1], 4, WidestOddExpanded[group[i] - 1], false);
                charWidths[i, 0] = RSSWidths[0];
                charWidths[i, 2] = RSSWidths[1];
                charWidths[i, 4] = RSSWidths[2];
                charWidths[i, 6] = RSSWidths[3];

                GetRSSWidths(vEven[i], EvenModulesExpanded[group[i] - 1], 4, WidestEvenExpanded[group[i] - 1], true);
                charWidths[i, 1] = RSSWidths[0];
                charWidths[i, 3] = RSSWidths[1];
                charWidths[i, 5] = RSSWidths[2];
                charWidths[i, 7] = RSSWidths[3];
            }

            // 7.2.6 Check character.
            // The checksum value is equal to the mod 211 residue of the weighted sum of the RSSWidths of the
            // elements in the data characters.
            int checksum = 0;
            int checkGroup = 0;
            for (int i = 0; i < dataCharacters; i++)
            {
                int r = RowWeights[(((dataCharacters - 2) / 2) * 21) + i];
                for (int j = 0; j < 8; j++)
                    checksum += charWidths[i, j] * ChecksumWeightExpanded[(r * 8) + j];
            }

            int checkCharacter = (211 * ((dataCharacters + 1) - 4)) + (checksum % 211);

            if (checkCharacter <= 347)
                checkGroup = 1;

            if (checkCharacter >= 348 && checkCharacter <= 1387)
                checkGroup = 2;

            if (checkCharacter >= 1388 && checkCharacter <= 2947)
                checkGroup = 3;

            if (checkCharacter >= 2948 && checkCharacter <= 3987)
                checkGroup = 4;

            if (checkCharacter >= 3988)
                checkGroup = 5;

            int checkOdd = (checkCharacter - GroupSumExpanded[checkGroup - 1]) / EvenExpanded[checkGroup - 1];
            int checkEven = (checkCharacter - GroupSumExpanded[checkGroup - 1]) % EvenExpanded[checkGroup - 1];

            GetRSSWidths(checkOdd, OddModulesExpanded[checkGroup - 1], 4, WidestOddExpanded[checkGroup - 1], false);
            checkWidths[0] = RSSWidths[0];
            checkWidths[2] = RSSWidths[1];
            checkWidths[4] = RSSWidths[2];
            checkWidths[6] = RSSWidths[3];

            GetRSSWidths(checkEven, EvenModulesExpanded[checkGroup - 1], 4, WidestEvenExpanded[checkGroup - 1], true);
            checkWidths[1] = RSSWidths[0];
            checkWidths[3] = RSSWidths[1];
            checkWidths[5] = RSSWidths[2];
            checkWidths[7] = RSSWidths[3];

            // Initialise element array.
            int patternWidth = ((((dataCharacters + 1) / 2) + ((dataCharacters + 1) & 1)) * 5) + ((dataCharacters + 1) * 8) + 4;
            int[] elements = new int[patternWidth];

            // Put finder patterns in element array.
            for (int i = 0; i < ((dataCharacters + 1) / 2) + ((dataCharacters + 1) & 1); i++)
            {
                int index = ((((((dataCharacters + 1) - 2) / 2) + ((dataCharacters + 1) & 1)) - 1) * 11) + i;
                for (int j = 0; j < 5; j++)
                    elements[(21 * i) + j + 10] = FinderPatternExpanded[((FinderSequence[index] - 1) * 5) + j];
            }

            // Put check character in element array.
            for (int i = 0; i < 8; i++)
                elements[i + 2] = checkWidths[i];

            // Put forward reading data characters in element array.
            for (int i = 1; i < dataCharacters; i += 2)
            {
                for (int j = 0; j < 8; j++)
                    elements[(((i - 1) / 2) * 21) + 23 + j] = charWidths[i, j];
            }

            // Put reversed data characters in element array.
            for (int i = 0; i < dataCharacters; i += 2)
            {
                for (int j = 0; j < 8; j++)
                    elements[((i / 2) * 21) + 15 + j] = charWidths[i, 7 - j];
            }

            // Build the elements into the symbol.
            if (symbolId == Symbology.DatabarExpanded)
                BuildExpandedSymbol(elements, patternWidth);

            else
                BuildExpandedStackedSymbol(elements, dataCharacters, patternWidth);
        }

        private void BuildExpandedSymbol(int[] elements, int patternWidth)
        {
            // Builds the symbol for databar expanded.
            byte[] rowData;
            SymbolData symbolData;

            bool latch = false;
            int symbolWidth = 0;
            int position = 0;

            // Insert the row start and stop.
            elements[0] = 1;
            elements[1] = 1;
            elements[patternWidth - 2] = 1;
            elements[patternWidth - 1] = 1;

            // Get the total length of the bit pattern.
            for (int i = 0; i < patternWidth; i++)
                symbolWidth += elements[i];

            rowData = new byte[symbolWidth];
            for (int i = 0; i < patternWidth; i++)
            {
                for (int j = 0; j < elements[i]; j++)
                {
                    if (latch)
                        rowData[position] = 1;

                    position++;
                }

                latch = !latch;
            }

            symbolData = new SymbolData(rowData, 34.0f);   // Databar Expanded height = 34X.
            Symbol.Add(symbolData);

            // Build the symbol separator and add the 2D component.
            if (isCompositeSymbol)
            {
                rowData = new byte[symbolWidth];
                for (int i = 4; i < symbolWidth - 4; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                        rowData[i] = 1;
                }

                // Finder bar adjustment.
                int column = symbolWidth / 49;
                FinderAdjustment(rowData, column, 0, false, true);
                // Insert the separator to the symbol (top down).
                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(0, symbolData);
                CompositeEncoder.AddComposite(symbolId, compositeMessage, Symbol, compositeMode, symbolWidth);
            }
        }

        /// <summary>
        /// Builds the Databar Expanded Stacked symbol.
        /// </summary>
        /// <remarks>builds the symbol top down</remarks>
        /// <param name="elements"></param>
        /// <param name="dataCharacters"></param>
        /// <param name="patternWidth"></param>
        private void BuildExpandedStackedSymbol(int[] elements, int dataCharacters, int patternWidth)
        {
            SymbolData symbolData;
            byte[] rowData;
            int[] subElements;
            int row;
            int currentRow;
            int position;
            int symbolWidth;
            bool isSpecialRow;
            bool isLeftToRight;
            bool latch = false;
            int column = 0;
            int columnsPerRow = segments / 2;
            int maxWidth = 0;

            int codeBlocks = ((dataCharacters + 1) / 2) + ((dataCharacters + 1) % 2);
            if (columnsPerRow > codeBlocks) // User supplied segments is large than needed to encode symbol.
                columnsPerRow = codeBlocks;

            int stackedRows = (codeBlocks / columnsPerRow) + (codeBlocks % columnsPerRow > 0 ? 1 : 0);
            int subElementCount = 0;
            int currentBlock = 0;

            for (currentRow = 1; currentRow <= stackedRows; currentRow++)
            {
                isSpecialRow = false;
                subElements = new int[500];

                // Row start.
                subElements[0] = 1;
                subElements[1] = 1;
                subElementCount = 2;

                // Row Data.
                column = 0;
                do
                {
                    if ((((columnsPerRow & 1) != 0) || (currentRow & 1) != 0) ||
                        (currentRow == stackedRows && codeBlocks != (currentRow * columnsPerRow) &&
                        ((((currentRow * columnsPerRow) - codeBlocks) & 1) != 0)))
                    {
                        // Left to right.
                        isLeftToRight = true;
                        int i = 2 + (currentBlock * 21);
                        for (int j = 0; j < 21; j++)
                        {
                            if ((i + j) < patternWidth)
                                subElements[j + (column * 21) + 2] = elements[i + j];

                            subElementCount++;
                        }
                    }

                    else
                    {
                        // Right to left.
                        isLeftToRight = false;
                        int i = 2 + (((currentRow * columnsPerRow) - column - 1) * 21);
                        for (int j = 0; j < 21; j++)
                        {
                            if ((i + j) < patternWidth)
                                subElements[(20 - j) + (column * 21) + 2] = elements[i + j];

                            subElementCount++;
                        }
                    }

                    column++;
                    currentBlock++;
                }
                while (column < columnsPerRow && currentBlock < codeBlocks);

                // Row stop.
                subElements[subElementCount] = 1;
                subElements[subElementCount + 1] = 1;
                subElementCount += 2;

                latch = !((currentRow & 1) != 0);

                if (currentRow == stackedRows && codeBlocks != (currentRow * columnsPerRow) &&
                    ((((currentRow * columnsPerRow) - codeBlocks) & 1) != 0) && ((columnsPerRow & 1) == 0))
                {
                    // Special case bottom row.
                    isSpecialRow = true;
                    subElements[0] = 2;
                    latch = false;
                }

                symbolWidth = 0;
                position = 0;
                for (int i = 0; i < subElementCount; i++)
                    symbolWidth += subElements[i];

                rowData = new byte[symbolWidth];
                for (int i = 0; i < subElementCount; i++)
                {
                    for (int j = 0; j < subElements[i]; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }

                symbolData = new SymbolData(rowData, 34.0f);    // Databar Expanded height = 34X.
                Symbol.Add(symbolData);

                if (currentRow != 1)
                {
                    // Middle separator pattern (above current row).
                    int length = 49 * columnsPerRow;
                    rowData = new byte[length + 1];
                    row = Symbol.Count - 1;

                    for (int j = 5; j < length; j += 2)
                        rowData[j] = 1;

                    symbolData = new SymbolData(rowData, 1.0f);
                    Symbol.Insert(row, symbolData);

                    // Bottom separator pattern (above current row).
                    rowData = new byte[symbolWidth];
                    row = Symbol.Count - 1;

                    for (int j = 4; j < (symbolWidth - 4); j++)
                    {
                        if (Symbol[row].GetRowData()[j] == 0)
                            rowData[j] = 1;
                    }

                    FinderAdjustment(rowData, column, row, isSpecialRow, isLeftToRight);
                    symbolData = new SymbolData(rowData, 1.0f);
                    Symbol.Insert(row, symbolData);
                }

                if (currentRow != stackedRows)
                {
                    // Top separator pattern (below previous row).
                    rowData = new byte[symbolWidth];
                    row = Symbol.Count - 1;

                    for (int j = 4; j < (symbolWidth - 4); j++)
                    {
                        if (Symbol[row].GetRowData()[j] == 0)
                            rowData[j] = 1;
                    }

                    FinderAdjustment(rowData, column, row, isSpecialRow, isLeftToRight);
                    symbolData = new SymbolData(rowData, 1.0f);
                    Symbol.Add(symbolData);
                }

                maxWidth = Math.Max(maxWidth, symbolWidth);
            }

            if (isCompositeSymbol)
            {
                // 2D separator pattern.
                rowData = new byte[maxWidth];
                for (int i = 4; i < maxWidth - 4; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                        rowData[i] = 1;
                }

                FinderAdjustment(rowData, column, 0, false, true);
                // Insert the separator into the symbol (top down ).
                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(0, symbolData);
                // Add 2D component.
                CompositeEncoder.AddComposite(symbolId, compositeMessage, Symbol, compositeMode, maxWidth);
            }
        }

        private void FinderAdjustment(byte[] rowData, int column, int row, bool isSpecialRow, bool isLeftToRight)
        {
            for (int j = 0; j < column; j++)
            {
                int k = (49 * j) + ((isSpecialRow) ? 19 : 18);
                if (isLeftToRight)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        if (Symbol[row].GetRowData()[i + k - 1] == 0 && Symbol[row].GetRowData()[i + k] == 0 && rowData[i + k - 1] == 1)
                            rowData[i + k] = 0;
                    }
                }

                else
                {
                    for (int i = 14; i >= 0; i--)
                    {
                        if (Symbol[row].GetRowData()[i + k + 1] == 0 && Symbol[row].GetRowData()[i + k] == 0 && rowData[i + k + 1] == 1)
                            rowData[i + k] = 0;
                    }
                }
            }
        }
    }
}