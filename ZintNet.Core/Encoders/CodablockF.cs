/* Codablock.c - Handles Codablock-F and Codablock-E 2D barcode */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Harald Oehlmann and other Zint Authors and Contributors.
   
    libzint - the open source barcode library
    Copyright (C) 2016 Harald Oehlmann

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

using System.Diagnostics;

namespace ZintNet.Core.Encoders
{
    /// <summary>
    /// CodablockF analysing map.
    /// </summary>
    internal sealed class CharacterSetTable
    {
        public int CharacterSet = 0;   // Still possible character sets for actual.
        public int AFollowing = 0;     // Still following Characters in Charset A.
        public int BFollowing = 0;     // Still following Characters in Charset B.
        public int CFollowing = 0;     // Still following Characters in Charset C.
    }

    internal class CodaBlockEncoder : SymbolEncoder
    {
        #region Tables And Constants.
        // Number of bars per character. (plus 2 for end character)
        // private const int C128Elements = 11;
        // FTab C128 flags - may be added.
        private const int CodeA = 1;
        private const int CodeB = 2;
        private const int CodeC = 4;
        private const int CEnd = 8;
        private const int CShift = 16;
        private const int CFill = 32;
        private const int CodeFNC1 = 64;
        private const int ZTNum = (CodeA + CodeB + CodeC);
        private const int ZTFNC1 = (ZTNum + CodeFNC1);

        // ASCII-Extension for Codablock-F.
        private const byte aFNC1 = 128;
        private const byte aFNC2 = 129;
        private const byte aFNC3 = 130;
        private const byte aFNC4 = 131;
        private const byte aCodeA = 132;
        private const byte aCodeB = 133;
        private const byte aCodeC = 134;
        private const byte aShift = 135;

        private static string[] Code128Table = {
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
            "2331112" };
        #endregion

        private int codaBlockRows;
        private int codaBlockColumns;

        public CodaBlockEncoder(Symbology symbology, string barcodeMessage, int codaBlockRows, int codaBlockColumns)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.codaBlockRows = codaBlockRows;
            this.codaBlockColumns = codaBlockColumns;
            this.elementsPerCharacter = 11;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
            CodaBlockF();
            return Symbol;
        }

        private void CodaBlockF()
        {
            bool error;
            int usableColumns;
            int checksum1, checkSum2;
            int currentRows;
            int currentCharacter;
            int currentCharacterSet;
            int emptyColumns;
            byte[] data;
            int[] characterSet;
            byte[] symbolGrid;
            int gridPosition;
            int rows = codaBlockRows;
            int columns = codaBlockColumns;
            int fillings = 0;
            int dataLength = 0;
            StringBuilder rowPattern;
            int inputLength = barcodeData.Length;

            List<byte> dataList = new List<byte>();
            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] > 127)
                {
                    dataList.Add(aFNC4);
                    dataList.Add((byte)(barcodeData[i] & 127));
                }

                else
                    dataList.Add((byte)barcodeData[i]);
            }

            dataLength = dataList.Count;
            data = dataList.ToArray();
            characterSet = new int[dataLength];
            
            Collection<CharacterSetTable> characterSetTables = new Collection<CharacterSetTable>();
            for (int i = 0; i < dataLength; i++)
                characterSetTables.Add(new CharacterSetTable());

            CreateCharacterSetTable(characterSetTables, data, dataLength);
            // Find final row and column count.
            // Nor row nor column count given.
            if (rows <= 0 && columns <= 5)
            {
                // Use Code128 until reasonable size.
                if (dataLength < 9)
                    rows = 1;

                else
                {
                    // Use 1/1 aspect/ratio Codablock.
                    columns = ((int)Math.Floor(Math.Sqrt(1.0 * dataLength)) + 5);
                    if (columns > 64)
                        columns = 64;
                }
            }

            // There are 5 Codewords for Organisation Start(2), row(1), CheckSum & Stop.
            usableColumns = columns - 5;
            if (rows > 0)  // Row count given.
                error = RowsToColumns(characterSetTables, dataLength, ref rows, ref usableColumns, characterSet, ref fillings);

            else  // Column count given.
                error = ColumnsToRows(characterSetTables, dataLength, ref rows, ref usableColumns, characterSet, ref fillings);

            if (error == true)
                throw new InvalidDataLengthException("Codablock-F: Input data too long.");

            // Checksum.
            checksum1 = checkSum2 = 0;
            if (rows > 1)
            {
                for (int p = 0; p < dataLength; p++)
                {
                    checksum1 = (checksum1 + (p % 86 + 1) * data[p]) % 86;
                    checkSum2 = (checkSum2 + (p % 86) * data[p]) % 86;
                }
            }

            columns = usableColumns + 5;
            symbolGrid = new byte[columns * rows];
            gridPosition = 0;
            currentCharacter = 0;
            // Loop over rows.
            for (currentRows = 0; currentRows < rows; currentRows++)
            {
                if (currentCharacter >= dataLength)
                {
                    // Empty line with StartCCodeBCodeC.
                    currentCharacterSet = CodeC;
                    symbolGrid[gridPosition] = 0x67;
                    gridPosition++;
                    symbolGrid[gridPosition] = 0x63;
                    gridPosition++;
                    SumASCII(symbolGrid, ref gridPosition, currentRows + 42, CodeC);
                    emptyColumns = usableColumns - 2;
                    while (emptyColumns > 0)
                    {
                        if (currentCharacterSet == CodeC)
                        {
                            A2C128_C(symbolGrid, ref gridPosition, aCodeB, 0);
                            currentCharacterSet = CodeB;
                        }

                        else
                        {
                            A2C128_B(symbolGrid, ref gridPosition, aCodeC);
                            currentCharacterSet = CodeC;
                        }

                        emptyColumns--;
                    }
                }

                else
                {
                    // Normal Line.
                    // Startcode.
                    switch (characterSet[currentCharacter] & (CodeA + CodeB + CodeC))
                    {
                        case CodeA:
                            symbolGrid[gridPosition] = 0x67;
                            gridPosition++;
                            if (rows > 1)
                            {
                                symbolGrid[gridPosition] = 0x62;
                                gridPosition++;
                            }

                            currentCharacterSet = CodeA;
                            break;

                        case CodeB:
                            if (rows == 1)
                            {
                                symbolGrid[gridPosition] = 0x68;
                                gridPosition++;
                            }

                            else
                            {
                                symbolGrid[gridPosition] = 0x67;
                                gridPosition++;
                                symbolGrid[gridPosition] = 0x64;
                                gridPosition++;
                            }

                            currentCharacterSet = CodeB;
                            break;

                        case CodeC:
                        default:
                            if (rows == 1)
                            {
                                symbolGrid[gridPosition] = 0x69;
                                gridPosition++;
                            }

                            else
                            {
                                symbolGrid[gridPosition] = 0x67;
                                gridPosition++;
                                symbolGrid[gridPosition] = 0x63;
                                gridPosition++;
                            }

                            currentCharacterSet = CodeC;
                            break;
                    }

                    if (rows > 1)
                    {
                        // Set F1
                        // In first line : # of rows
                        // In Case of CodeA we shifted to CodeB
                        SumASCII(symbolGrid, ref gridPosition, (currentRows == 0) ? rows - 2 : currentRows + 42, (currentCharacterSet == CodeA) ? CodeB : currentCharacterSet);
                    }

                    // Data 
                    emptyColumns = usableColumns;
                    // One liners don't have start/stop code.
                    if (rows == 1)
                        emptyColumns += 2;

                    while (emptyColumns > 0)
                    {
                        // Change character set
                        // not at first possition (It was then the start set)
                        // special case for one-liner
                        if (emptyColumns < usableColumns || (rows == 1 && currentCharacter != 0))
                        {
                            if ((characterSet[currentCharacter] & CodeA) != 0)
                            {
                                // Change to A.
                                ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, aCodeA, 0);
                                emptyColumns--;
                                currentCharacterSet = CodeA;
                            }

                            else if ((characterSet[currentCharacter] & CodeB) != 0)
                            {
                                // Change to B.
                                ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, aCodeB, 0);
                                emptyColumns--;
                                currentCharacterSet = CodeB;
                            }

                            else if ((characterSet[currentCharacter] & CodeC) != 0)
                            {
                                // Change to C.
                                ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, aCodeC, 0);
                                emptyColumns--;
                                currentCharacterSet = CodeC;
                            }
                        }

                        if ((characterSet[currentCharacter] & CShift) != 0)
                        {
                            // Shift it and put out the shifted character.
                            ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, aShift, 0);
                            emptyColumns -= 2;
                            currentCharacterSet = (currentCharacterSet == CodeB) ? CodeA : CodeB;
                            ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, data[currentCharacter], 0);
                            currentCharacterSet = (currentCharacterSet == CodeB) ? CodeA : CodeB;
                        }

                        else
                        {
                            // Normal character.
                            if (currentCharacterSet == CodeC)
                            {
                                if (data[currentCharacter] == aFNC1)
                                    A2C128_C(symbolGrid, ref gridPosition, aFNC1, 0);

                                else
                                {
                                    A2C128_C(symbolGrid, ref gridPosition, data[currentCharacter], data[currentCharacter + 1]);
                                    currentCharacter++;
                                }
                            }

                            else
                                ASCIIZ128(symbolGrid, ref gridPosition, currentCharacterSet, data[currentCharacter], 0);

                            emptyColumns--;
                        }

                        // End Criteria.
                        if ((characterSet[currentCharacter] & CFill) != 0)
                        {
                            // Fill Line but leave space for checks in last line.
                            if (currentRows == rows - 1 && emptyColumns >= 2)
                                emptyColumns -= 2;

                            while (emptyColumns > 0)
                            {
                                switch (currentCharacterSet)
                                {
                                    case CodeC:
                                        A2C128_C(symbolGrid, ref gridPosition, aCodeB, 0);
                                        currentCharacterSet = CodeB;
                                        break;

                                    case CodeB:
                                        A2C128_B(symbolGrid, ref gridPosition, aCodeC);
                                        currentCharacterSet = CodeC;
                                        break;

                                    case CodeA:
                                        A2C128_A(symbolGrid, ref gridPosition, aCodeC);
                                        currentCharacterSet = CodeC;
                                        break;
                                }

                                emptyColumns--;
                            }
                        }

                        if ((characterSet[currentCharacter] & CEnd) != 0)
                            emptyColumns = 0;

                        currentCharacter++;
                    }
                }

                // Add checksum in last line.
                if (rows > 1 && currentRows == rows - 1)
                {
                    SumASCII(symbolGrid, ref gridPosition, checksum1, currentCharacterSet);
                    SumASCII(symbolGrid, ref gridPosition, checkSum2, currentCharacterSet);
                }

                // Add Code 128 checksum.
                {
                    int code128Checksum = 0;
                    int position = 0;
                    for (; position < usableColumns + 3; position++)
                        code128Checksum = (code128Checksum + ((position == 0 ? 1 : position) * symbolGrid[columns * currentRows + position]) % 103) % 103;

                    symbolGrid[gridPosition] = (byte)code128Checksum;
                    gridPosition++;
                }

                // Add terminaton character.
                symbolGrid[gridPosition] = 106;
                gridPosition++;
            }

            Debug.WriteLine("\nCode 128 Code Numbers:");
            {  
                int DPos, DPos2;
                for (DPos = 0; DPos < rows; DPos++)
                {
                    for (DPos2 = 0; DPos2 < columns; DPos2++)
                        Debug.Write(String.Format("{0,4}", symbolGrid[DPos * columns + DPos2]));

                    Debug.Write("\n");
                }
            }
            Debug.WriteLine(String.Format("rows={0} columns={1} fillings={2}", rows, columns, fillings));

            // Build the symbol.
            for (int r = 0; r < rows; r++)
            {
                rowPattern = new StringBuilder();
                for (int c = 0; c < columns; c++)
                    rowPattern.Append(Code128Table[symbolGrid[r * columns + c]]);

                // Expand row into the symbol data..
                SymbolBuilder.ExpandSymbolRow(Symbol, rowPattern, 10.0f);
            }
        }

        /// <summary>
        /// Creates a table for each data charater.
        /// </summary>
        /// <remarks>
        /// int CharacterSet: is an or of CodeA,CodeB,CodeC,CodeFNC1, in dependency which character set is applicable. (Result of GetPossibleCharacterSet)
        /// int AFollowing, BFollowing: The number of source characters you still may encode in this character set.
        /// int CFollowing: The number of characters encodable in CodeC if we start here.
        /// </remarks>
        /// <param name="characterSetTables"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        private static void CreateCharacterSetTable(Collection<CharacterSetTable> characterSetTables, byte[] data, int dataLength)
        {
            int currentCharacter;
            int runCharacter;

            // Treat the Data backwards.
            currentCharacter = dataLength - 1;
            characterSetTables[currentCharacter].CharacterSet = GetPossibleCharacterSet(data[currentCharacter]);
            characterSetTables[currentCharacter].AFollowing = ((characterSetTables[currentCharacter].CharacterSet & CodeA) == 0) ? 0 : 1;
            characterSetTables[currentCharacter].BFollowing = ((characterSetTables[currentCharacter].CharacterSet & CodeB) == 0) ? 0 : 1;
            characterSetTables[currentCharacter].CFollowing = 0;

            for (currentCharacter--; currentCharacter >= 0; currentCharacter--)
            {
                characterSetTables[currentCharacter].CharacterSet = GetPossibleCharacterSet(data[currentCharacter]);
                characterSetTables[currentCharacter].AFollowing = ((characterSetTables[currentCharacter].CharacterSet & CodeA) == 0) ? 0 : characterSetTables[currentCharacter + 1].AFollowing + 1;
                characterSetTables[currentCharacter].BFollowing = ((characterSetTables[currentCharacter].CharacterSet & CodeB) == 0) ? 0 : characterSetTables[currentCharacter + 1].BFollowing + 1;
                characterSetTables[currentCharacter].CFollowing = 0;
            }

            // Find the CodeC-chains.
            for (currentCharacter = 0; currentCharacter < dataLength; currentCharacter++)
            {
                characterSetTables[currentCharacter].CFollowing = 0;
                if ((characterSetTables[currentCharacter].CharacterSet & CodeC) != 0)
                {
                    // CodeC possible.
                    runCharacter = currentCharacter;
                    do
                    {
                        // Whether this is FNC1 whether next is numeric.
                        if (characterSetTables[runCharacter].CharacterSet == ZTFNC1) // FNC1.
                            characterSetTables[currentCharacter].CFollowing++;

                        else
                        {
                            runCharacter++;
                            if (runCharacter >= dataLength)
                                break;

                            // Only a Number may follow.
                            if (characterSetTables[runCharacter].CharacterSet == ZTNum)
                                characterSetTables[currentCharacter].CFollowing += 2;

                            else
                                break;
                        }

                        ++runCharacter;
                    } while (runCharacter < dataLength);
                }
            }
        }

        /// <summary>
        /// Find the possible Code-128 Character sets for a character
        /// </summary>
        /// <remarks>
        /// The result is an or of CodeA, CodeB, CodeC, CodeFNC1 in dependency of the
        /// possible Code 128 character sets.
        /// </remarks>
        /// <param name="currentCharacter"></param>
        /// <returns></returns>

        private static int GetPossibleCharacterSet(byte currentCharacter)
        {
            if (currentCharacter <= 0x19)
                return CodeA;

            if (currentCharacter >= '0' && currentCharacter <= '9')
                return ZTNum;           // ZTNum=CodeA+CodeB+CodeC.

            if (currentCharacter == aFNC1)
                return ZTFNC1;          // ZTFNC1=CodeA+CodeB+CodeC+CodeFNC1.

            if (currentCharacter >= 0x60 && currentCharacter <= 0x7f)      // 60 to 127.
                return CodeB;

            return CodeA + CodeB;
        }

        private static bool RowsToColumns(Collection<CharacterSetTable> characterSetTables, int dataLength,
            ref int rows, ref int usableColumns, int[] characterSet, ref int fillings)
        {
            bool error;
            int rowsRequested;  // Number of requested rows.
            int useColumns;     // Usable columns.
            int testColumns;    // To enter into ColumnToRows.
            int currentRows = 0;
            int backupRows = 0;
            int tempFillings = 0;
            int backupFillings = 0;
            int backupColumns = 0;
            bool backupOk = false;      // The memorysed set is ok.
            int testListSize = 0;
            int[] testList = new int[62];
            int[] backupSet = new int[dataLength];

            rowsRequested = rows;  // One liners are self-calibrating.
            if (rowsRequested == 1)
                testColumns = 32767;

            else
            {
                // First guess.
                testColumns = dataLength / rowsRequested;
                if (testColumns > 62)
                    testColumns = 62;

                else if (testColumns < 1)
                    testColumns = 1;
            }

            for (; ;)
            {
                testList[testListSize] = testColumns;
                testListSize++;
                useColumns = testColumns;   // Make a copy because it may be modified.
                error = ColumnsToRows(characterSetTables, dataLength, ref currentRows, ref useColumns, characterSet, ref tempFillings);
                if (error == true)
                    return error;

                if (currentRows <= rowsRequested)
                {
                    // Less or exactly line number found.
                    // Check if column count below already tested or Count = 1.
                    bool fInTestList = (currentRows == 1 || testColumns == 1);
                    int posCur;
                    for (posCur = 0; posCur < testListSize && !fInTestList; posCur++)
                    {
                        if (testList[posCur] == testColumns - 1)
                            fInTestList = true;
                    }

                    if (fInTestList)
                    {
                        /* Smaller Width already tested
                         * if rowsCur=rowsRequested->Exit
                         * if rowsCur<rowsRequested and fillings>0
                         * -> New search for rowsRequested:=rowsCur
                         */
                        if (currentRows == rowsRequested || tempFillings == 0 || testColumns == 1)
                        {
                            // Exit with actual.
                            fillings = tempFillings;
                            rows = currentRows;
                            usableColumns = useColumns;
                            return false;
                        }

                        // Search again for smaller line number.
                        rowsRequested = currentRows;
                        testList[0] = testColumns;
                        testListSize = 1;
                    }

                    // Test more rows (shorter CDB).
                    backupOk = (currentRows == rowsRequested);
                    Array.Copy(characterSet, backupSet, dataLength);
                    backupFillings = tempFillings;
                    backupColumns = useColumns;
                    backupRows = currentRows;
                    testColumns--;
                }

                else
                {
                    // To many rows.
                    bool fInTestList = backupOk;
                    int posCur;
                    for (posCur = 0; posCur < testListSize && !fInTestList; posCur++)
                    {
                        if (testList[posCur] == testColumns + 1)
                            fInTestList = true;
                    }

                    if (fInTestList)
                    {
                        // The next less-rows (larger) code was already tested. So give the larger back.
                        Array.Copy(backupSet, characterSet, dataLength);
                        fillings = backupFillings;
                        rows = backupRows;
                        usableColumns = backupColumns;
                        return false;
                    }

                    // Test less rows (longer code).
                    backupRows = currentRows;
                    Array.Copy(characterSet, backupSet, dataLength);
                    backupFillings = tempFillings;
                    backupColumns = useColumns;
                    backupOk = false;
                    testColumns++;
                }
            }
        }

        private static bool ColumnsToRows(Collection<CharacterSetTable> characterSetTables, int dataLength,
            ref int rows, ref int usableColumns, int[] characterSet, ref int fillings)
        {
            int useColumns;         // Usable Characters per line.
            int currentRows;
            int currentCharcter;
            int runCharacter;
            int emptyColumns;       // Number of codes still empty in line.
            int emptyColumns2;      // Alternative emptyColumns to compare.
            bool oneLiner;          // Flag if one liner.
            int codeCPairs;         // Number of digit pairs which may fit in the line.
            int currentCharSet;     // Current character set.
            int tempFillings = 0;   // Number of filling characters.

            useColumns = usableColumns;
            if (useColumns < 3)
                useColumns = 3;

            // Loop until rowsCur < 44.
            do
            {
                for (int i = 0; i < characterSet.Length; i++)
                    characterSet[i] = 0;

                currentCharcter = 0;
                currentRows = 0;
                oneLiner = true;        // First try one-Liner.

                // Line and OneLiner-try Loop.
                do
                {
                    // Start Character.
                    emptyColumns = useColumns;    // Remained place in Line.
                    if (oneLiner)
                        emptyColumns += 2;

                    // Choose in Set A or B.
                    // (C is changed as an option later on)
                    characterSet[currentCharcter] = currentCharSet = (characterSetTables[currentCharcter].AFollowing > characterSetTables[currentCharcter].BFollowing) ? CodeA : CodeB;

                    // Test on Numeric Mode C.
                    codeCPairs = RemainingDigits(characterSetTables, currentCharcter, emptyColumns);
                    if (codeCPairs >= 4)
                    {
                        // 4 Digits in Numeric, compression OK.
                        /* May be an odd start find more
                           Skip leading <FNC1>'s 
                           Typical structure : <FNC1><FNC1>12... 
                           Test if numeric after one isn't better.*/
                        runCharacter = currentCharcter;
                        emptyColumns2 = emptyColumns;
                        while (characterSetTables[runCharacter].CharacterSet == ZTFNC1)
                        {
                            runCharacter++;
                            emptyColumns2--;
                        }

                        if (codeCPairs >= RemainingDigits(characterSetTables, runCharacter + 1, emptyColumns2 - 1))
                        {
                            // Start odd is not better.
                            // We start in C.
                            characterSet[currentCharcter] = currentCharSet = CodeC;
                            // Increment charCur.
                            if (characterSetTables[currentCharcter].CharacterSet != ZTFNC1)
                                currentCharcter++;      // 2 Num.Digits.
                        }
                    }

                    currentCharcter++;
                    emptyColumns--;

                    // Following characters.
                    while (emptyColumns > 0 && currentCharcter < dataLength)
                    {
                        switch (currentCharSet)
                        {
                            case CodeA:
                            case CodeB:
                                // Check switching to Code C.
                                /* Switch if :
                                 *  - Character not FNC1
                                 *  - 4 real Digits will fit in line
                                 *  - an odd Start will not be better
                                 */
                                if (characterSetTables[currentCharcter].CharacterSet == ZTNum
                                    && (codeCPairs = RemainingDigits(characterSetTables, currentCharcter, emptyColumns - 1)) >= 4
                                    && codeCPairs > RemainingDigits(characterSetTables, currentCharcter + 1, emptyColumns - 2))
                                {
                                    // Change to C.
                                    characterSet[currentCharcter] = currentCharSet = CodeC;
                                    currentCharcter += 2;   // 2 Digit.
                                    emptyColumns -= 2;      // <SwitchC> 12.
                                }

                                else if (currentCharSet == CodeA)
                                {
                                    if (characterSetTables[currentCharcter].AFollowing == 0)
                                    {
                                        // Must change to B.
                                        if (emptyColumns == 1)
                                        {
                                            // Can't switch.
                                            characterSet[currentCharcter - 1] |= CEnd + CFill;
                                            emptyColumns = 0;
                                        }

                                        else
                                        {
                                            // <Shift> or <switchB>.
                                            if (characterSetTables[currentCharcter].BFollowing == 1)
                                                characterSet[currentCharcter] |= CShift;

                                            else
                                            {
                                                characterSet[currentCharcter] |= CodeB;
                                                currentCharSet = CodeB;
                                            }

                                            emptyColumns -= 2;
                                            currentCharcter++;
                                        }
                                    }

                                    else
                                    {
                                        emptyColumns--;
                                        currentCharcter++;
                                    }
                                }

                                else
                                {
                                    // Last possibility, CodeB.
                                    if (characterSetTables[currentCharcter].BFollowing == 0)
                                    {
                                        // Must change to A.
                                        if (emptyColumns == 1)
                                        {
                                            // Can't switch.
                                            characterSet[currentCharcter - 1] |= CEnd + CFill;
                                            emptyColumns = 0;
                                        }

                                        else
                                        {
                                            // <Shift> or <switchA>.
                                            if (characterSetTables[currentCharcter].AFollowing == 1)
                                                characterSet[currentCharcter] |= CShift;

                                            else
                                            {
                                                characterSet[currentCharcter] |= CodeA;
                                                currentCharSet = CodeA;
                                            }

                                            emptyColumns -= 2;
                                            currentCharcter++;
                                        }
                                    }

                                    else
                                    {
                                        emptyColumns--;
                                        currentCharcter++;
                                    }
                                }

                                break;

                            case CodeC:
                                if (characterSetTables[currentCharcter].CFollowing > 0)
                                {
                                    currentCharcter += (characterSetTables[currentCharcter].CharacterSet == ZTFNC1) ? 1 : 2;
                                    emptyColumns--;
                                }

                                else
                                {
                                    // Must change to A or B.
                                    if (emptyColumns == 1)
                                    {
                                        // Can't switch.
                                        characterSet[currentCharcter - 1] |= CEnd + CFill;
                                        emptyColumns = 0;
                                    }

                                    else
                                    {
                                        // <SwitchA> or <switchA>.
                                        currentCharSet = characterSet[currentCharcter] = (characterSetTables[currentCharcter].AFollowing > 
                                            characterSetTables[currentCharcter].BFollowing) ? CodeA : CodeB;
                                        emptyColumns -= 2;
                                        currentCharcter++;
                                    }
                                }

                                break;
                        } 
                    }

                    // End of code line.
                    characterSet[currentCharcter - 1] |= CEnd;
                    currentRows++;
                    if (oneLiner)
                    {
                        if (currentCharcter < dataLength)
                        {
                            // One line not sufficiant.
                            oneLiner = false;
                            // Reset and Start again.
                            currentCharcter = 0;
                            currentRows = 0;
                            for (int i = 0; i < characterSet.Length; i++)
                                characterSet[i] = 0;
                        }

                        else    // Calculate real length of a one liner (-2 Based!!)
                            useColumns -= emptyColumns;
                    }

                } while (currentCharcter < dataLength); 

                // Place check characters C1, C2.
                if (oneLiner)
                    tempFillings = 0;

                else
                {
                    switch (emptyColumns)
                    {
                        case 1:
                            characterSet[currentCharcter - 1] |= CFill;
                            currentRows++;
                            tempFillings = useColumns - 2 + emptyColumns;
                            /* Glide in following block without break */    // not allowed in c#
                            break; 

                        case 0:
                            currentRows++;
                            tempFillings = useColumns - 2 + emptyColumns;
                            break;

                        case 2:
                            tempFillings = 0;
                            break;

                        default:
                            characterSet[currentCharcter - 1] |= CFill;
                            tempFillings = emptyColumns - 2;
                            break;
                    }
                }

                if (currentRows > 44)
                {
                    useColumns++;
                    if (useColumns > 62)
                        throw new InvalidDataLengthException("Codablock-F: Input data too long.");
                }
            } while (currentRows > 44);

            usableColumns = useColumns;
            rows = currentRows;
            fillings = tempFillings;
            return false;
        }

        /* Find the number of numerical characters in pairs which will fit in
         * one bundle into the line (up to here). This is calculated online because
         * it depends on the space in the line.
         */
        private static int RemainingDigits(Collection<CharacterSetTable> characterSetTables, int currentCharacter, int emptyColumns)
        {
            int digitCount;     // Numerical digits fitting in the line.
            int runCharacter = currentCharacter;
            digitCount = 0;

            while (emptyColumns > 0 && runCharacter < currentCharacter + characterSetTables[currentCharacter].CFollowing)
            {
                if (characterSetTables[runCharacter].CharacterSet != ZTFNC1)
                {
                    // NOT FNC1.
                    digitCount += 2;
                    runCharacter++;
                }

                runCharacter++;
                emptyColumns--;
            }

            return digitCount;
        }

        // Output a character in Characterset.
        private static void ASCIIZ128(byte[] symbolGrid, ref int gridPosition, int CharacterSet, byte c1, byte c2)
        {
            if (CharacterSet == CodeA)
                A2C128_A(symbolGrid, ref gridPosition, c1);

            else if (CharacterSet == CodeB)
                A2C128_B(symbolGrid, ref gridPosition, c1);
            else
                A2C128_C(symbolGrid, ref gridPosition, c1, c2);
        }

        // XLate Table A of Codablock-F Specification and call output.
        private static void SumASCII(byte[] symbolGrid, ref int gridPosition, int sum, int characterSet)
        {
            switch (characterSet)
            {
                case CodeA:
                    A2C128_A(symbolGrid, ref gridPosition, (byte)sum);
                    break;

                case CodeB:
                    if (sum <= 31)
                        A2C128_B(symbolGrid, ref gridPosition, (byte)(sum + 96));

                    else if (sum <= 47)
                        A2C128_B(symbolGrid, ref gridPosition, (byte)sum);

                    else
                        A2C128_B(symbolGrid, ref gridPosition, (byte)(sum + 10));
                    break;

                case CodeC:
                    A2C128_C(symbolGrid, ref gridPosition, (byte)(sum / 10 + '0'), (byte)(sum % 10 + '0'));
                    break;
            }
        }

        // Print a character in character set A.
        private static void A2C128_A(byte[] symbolGrid, ref int gridPosition, byte c)
        {
            switch (c)
            {
                case aCodeB: symbolGrid[gridPosition] = 100; break;
                case aFNC4: symbolGrid[gridPosition] = 101; break;
                case aFNC1: symbolGrid[gridPosition] = 102; break;
                case aFNC2: symbolGrid[gridPosition] = 97; break;
                case aFNC3: symbolGrid[gridPosition] = 96; break;
                case aCodeC: symbolGrid[gridPosition] = 99; break;
                case aShift: symbolGrid[gridPosition] = 98; break;
                default:
                    if (c >= ' ' && c <= '_')
                        symbolGrid[gridPosition] = (byte)(c - ' ');

                    else
                        symbolGrid[gridPosition] = (byte)(c + 64);

                    break;
            }

            gridPosition++;
        }


        // Output c in Set B
        private static void A2C128_B(byte[] symbolGrid, ref int gridPosition, byte c)
        {
            switch (c)
            {
                case aFNC1: symbolGrid[gridPosition] = 102; break;
                case aFNC2: symbolGrid[gridPosition] = 97; break;
                case aFNC3: symbolGrid[gridPosition] = 96; break;
                case aFNC4: symbolGrid[gridPosition] = 100; break;
                case aCodeA: symbolGrid[gridPosition] = 101; break;
                case aCodeC: symbolGrid[gridPosition] = 99; break;
                case aShift: symbolGrid[gridPosition] = 98; break;
                default: symbolGrid[gridPosition] = (byte)(c - ' '); break;
            }

            gridPosition++;
        }

        // Output c1, c2 in Set C.
        private static void A2C128_C(byte[] symbolGrid, ref int gridPosition, byte c1, byte c2)
        {
            switch (c1)
            {
                case aFNC1: symbolGrid[gridPosition] = 102; break;
                case aCodeB: symbolGrid[gridPosition] = 100; break;
                case aCodeA: symbolGrid[gridPosition] = 101; break;
                default: symbolGrid[gridPosition] = (byte)(10 * (c1 - '0') + (c2 - '0')); break;
            }

            gridPosition++;
        }
    }
}
