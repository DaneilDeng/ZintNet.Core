/* PDF417Encoder.cs - Handles Code PDF417, PDF417 Truncated & Micro PDF417 2D symbols */

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
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Text.Extensions;

namespace ZintNet.Core.Encoders
{
    internal class PDF417Encoder : SymbolEncoder
    {
        private const int TEXT = 900;
        private const int BYTE = 901;
        private const int NUMBER = 902;

        private bool isTruncated;
        private int optionDataColumns;
        private int optionErrorCorrection;
        private int optionElementHeight;

        public PDF417Encoder(Symbology symbology, string barcodeMessage, int optionDataColumns,
            int optionErrorCorrection, int optionElementHeight, EncodingMode encodingMode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionDataColumns = optionDataColumns;
            this.optionErrorCorrection = optionErrorCorrection;
            this.optionElementHeight = optionElementHeight;
            this.encodingMode = encodingMode;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            if (encodingMode == EncodingMode.GS1)   // PDF417 does not support GS1.
                encodingMode = EncodingMode.Standard;

            switch (encodingMode)
            {
                case EncodingMode.GS1:
                    encodingMode = EncodingMode.Standard;   // PDF417 does not support GS1.
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    break;

                case EncodingMode.Standard:
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    break;

                case EncodingMode.HIBC:
                    barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                    break;
            }

            switch (symbolId)
            {
                case Symbology.PDF417:
                case Symbology.PDF417Truncated:
                    isTruncated = symbolId == Symbology.PDF417 ? false : true;
                    PDF417();
                    break;

                case Symbology.MicroPDF417:
                    MicroPDF417();
                    break;
            }

            return Symbol;
        }

        private void PDF417()
        {
            int inputLength = barcodeData.Length;
            int modeListCount = 0;
            int offset;
            int eccLevel;

            int[,] modeList = new int[2, inputLength];
            List<int> dataStream = new List<int>();
            int[] eccStream;
            
            for (int i = 0; i < inputLength; i++)
            {
                int mode = GetMode(barcodeData[i]);
                if (i == 0)
                {
                    modeList[1, modeListCount] = mode;
                    modeList[0, modeListCount]++;
                }

                else
                {
                    if (mode == modeList[1, modeListCount])
                        modeList[0, modeListCount]++;

                    else
                    {
                        modeListCount++;
                        modeList[1, modeListCount] = mode;
                        modeList[0, modeListCount]++;
                    }
                }
            }

            modeListCount++;
            SmoothPDF(ref modeListCount, modeList);

            // Compress the data.
            int dataIndex = 0;
            int dataStreamLength;

            for (int i = 0; i < modeListCount; i++)
            {
                switch (modeList[1, i])
                {
                    case TEXT:
                        ProcessText(dataStream, dataIndex, modeList[0, i]);
                        break;

                    case BYTE:
                        ProcessByte(dataStream, dataIndex, modeList[0, i]);
                        break;

                    case NUMBER:
                        ProcessNumber(dataStream, dataIndex, modeList[0, i]);
                        break;
                }

                dataIndex += modeList[0, i];
            }

            // Now take care of the number of CWs per row.
            dataStreamLength = dataStream.Count;
            if (optionErrorCorrection < 0)
            {
                optionErrorCorrection = 6;
                if (dataStreamLength < 864)
                    optionErrorCorrection = 5;

                if (dataStreamLength < 321)
                    optionErrorCorrection = 4;

                if (dataStreamLength < 161)
                    optionErrorCorrection = 3;

                if (dataStreamLength < 41)
                    optionErrorCorrection = 2;
            }

            eccLevel = 1;
            for (int i = 1; i <= (optionErrorCorrection + 1); i++)
                eccLevel *= 2;

            if (optionDataColumns < 1)
                optionDataColumns = (int)(0.5 + Math.Sqrt((dataStreamLength + eccLevel) / 3.0));

            if (((dataStreamLength + eccLevel) / optionDataColumns) > 90)
                // Stop the symbol from becoming too high by increasing the columns.
                optionDataColumns = optionDataColumns + 1;

            if ((dataStreamLength + eccLevel) > 928)
                throw new InvalidDataLengthException("PDF417: Input data too long.");

            if (((dataStreamLength + eccLevel) / optionDataColumns) > 90)
                throw new InvalidDataLengthException("PDF417: Input data too long for specified number of columns.");

            // Padding calculation.
            int totalLength = dataStreamLength + eccLevel + 1;
            int padding = 0;
            if ((totalLength / optionDataColumns) < 3)    // A bar code must have at least three rows.
                padding = (optionDataColumns * 3) - totalLength;

            else
            {
                if ((totalLength % optionDataColumns) > 0)
                    padding = optionDataColumns - (totalLength % optionDataColumns);
            }

            // Add the padding.
            while (padding > 0)
            {
                dataStream.Add(900);
                padding--;
            }

            // Insert the length descriptor.
            dataStream.Insert(0, dataStream.Count + 1);

            // We now take care of the Reed Solomon codes.
            if (optionErrorCorrection == 0)
                offset = 0;

            else
                offset = eccLevel - 2;

            dataStreamLength = dataStream.Count;
            eccStream = new int[eccLevel];
            for (int i = 0; i < dataStreamLength; i++)
            {
                int total = (dataStream[i] + eccStream[eccLevel - 1]) % 929;
                for (int j = eccLevel - 1; j > 0; j--)
                    eccStream[j] = ((eccStream[j - 1] + 929) - (total * PDF417Tables.Coefficients[offset + j]) % 929) % 929;

                eccStream[0] = (929 - (total * PDF417Tables.Coefficients[offset]) % 929) % 929;
            }

            // Add the code words to the data stream.
            for (int i = eccLevel - 1; i >= 0; i--)
                dataStream.Add((eccStream[i] != 0) ? 929 - eccStream[i] : 0);

            dataStreamLength = dataStream.Count;
            int rowCount = dataStreamLength / optionDataColumns;
            if (dataStreamLength % optionDataColumns != 0)
                rowCount++;

            int c1 = (rowCount - 1) / 3;
            int c2 = (optionErrorCorrection * 3) + ((rowCount - 1) % 3);
            int c3 = optionDataColumns - 1;

            // Encode each row.
            StringBuilder codeString = new StringBuilder();
            BitVector bitPattern = new BitVector();
            int[] buffer = new int[optionDataColumns + 2];

            for (int row = 0; row < rowCount; row++)
            {
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;

                for (int i = 0; i < optionDataColumns; i++)
                    buffer[i + 1] = dataStream[row * optionDataColumns + i];

                int errorCorrection = (row / 3) * 30;
                switch (row % 3)
                {
                    /* Follows this codePattern from US Patent 5,243,655: 
                    Row 0: L0 (row #, # of rows)         R0 (row #, # of columns)
                    Row 1: L1 (row #, security level)    R1 (row #, # of rows)
                    Row 2: L2 (row #, # of columns)      R2 (row #, security level)
                    Row 3: L3 (row #, # of rows)         R3 (row #, # of columns)
                    etc. */
                    case 0:
                        buffer[0] = errorCorrection + c1;
                        buffer[optionDataColumns + 1] = errorCorrection + c3;
                        break;

                    case 1:
                        buffer[0] = errorCorrection + c2;
                        buffer[optionDataColumns + 1] = errorCorrection + c1;
                        break;

                    case 2:
                        buffer[0] = errorCorrection + c3;
                        buffer[optionDataColumns + 1] = errorCorrection + c2;
                        break;
                }

                codeString.Append("+*");
                if (isTruncated)
                {
                    // Truncated PDF - remove the last 5 characters.
                    for (int j = 0; j <= optionDataColumns; j++)
                    {
                        switch (row % 3)
                        {
                            case 1:
                                offset = 929;
                                break;

                            case 2:
                                offset = 1858;
                                break;

                            default:
                                offset = 0;
                                break;
                        }

                        codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[j]]);
                        codeString.Append("*");
                    }
                }

                else
                {
                    // Normal PDF417 symbol.
                    for (int j = 0; j <= optionDataColumns + 1; j++)
                    {
                        switch (row % 3)
                        {
                            case 1:
                                offset = 929;
                                break;

                            case 2:
                                offset = 1858;
                                break;

                            default: offset = 0;
                                break;
                        }

                        codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[j]]);
                        codeString.Append("*");
                    }

                    codeString.Append("-");
                }

                for (int i = 0; i < codeString.Length; i++)
                {
                    int position = PDF417Tables.PDFSet.IndexOf(codeString[i]);
                    if (position >= 0 && position < 32)
                        bitPattern.AppendBits(position, 5);

                    else if (position == 32)
                        bitPattern.AppendBits(1, 2);

                    else if (position == 33)
                        bitPattern.AppendBits(0xff54, 16);

                    else
                        bitPattern.AppendBits(0x1fa29, 17);
                }

                int size = bitPattern.SizeInBits;
                byte[] rowData = new byte[size];
                for (int i = 0; i < size; i++)
                    rowData[i] = bitPattern[i];

                SymbolData symbolData = new SymbolData(rowData, optionElementHeight);
                Symbol.Add(symbolData);

                // Clear data ready for next row.
                codeString.Clear();
                bitPattern.Clear();
            }
        }

        /// <summary>
        /// Determines the encoding mode for a particular character.
        /// </summary>
        /// <param name="asciiValue">character to test for mode</param>
        /// <returns>encoding mode for the current character</returns>
        private static int GetMode(char asciiValue)
        {
            int mode = BYTE;

            if (Char.IsDigit(asciiValue))
                mode = NUMBER;

            else if ((asciiValue == '\t') || (asciiValue == '\n') || (asciiValue == '\r') || ((asciiValue >= ' ') && (asciiValue <= '~')))
                mode = TEXT;

            return mode;
        }

        private static void CombineModeBlocks(ref int modeListCount, int[,] modeList)
        {
            int i, j;

            // Bring together same type blocks.
            if (modeListCount > 1)
            {
                i = 1;
                while (i < modeListCount)
                {
                    if (modeList[1, i - 1] == modeList[1, i])
                    {
                        // Bring together.
                        modeList[0, i - 1] = modeList[0, i - 1] + modeList[0, i];
                        j = i + 1;
                        //modeList[0, i] = 0; ///
                        //modeList[1, i] = 0;

                        // Decrease the list.
                        while (j < modeListCount)
                        {
                            modeList[0, j - 1] = modeList[0, j];
                            modeList[1, j - 1] = modeList[1, j];
                            j++;
                        }

                        modeListCount = modeListCount - 1;
                        i--;
                    }

                    i++;
                }
            }
        }

        private static void SmoothPDF(ref int modeListCount, int[,] modeList)
        {
            int length;
            int current, last, next;

            for (int i = 0; i < modeListCount; i++)
            {
                current = modeList[1, i];
                length = modeList[0, i];
                if (i != 0)
                    last = modeList[1, i - 1];

                else
                    last = 0;

                if (i != modeListCount - 1)
                    next = modeList[1, i + 1];

                else
                    next = 0;

                if (current == NUMBER)
                {
                    if (i == 0)
                    {
                        // First block.
                        if (modeListCount > 1)
                        {
                            // And there are others.
                            if ((next == TEXT) && (length < 8))
                                modeList[1, i] = TEXT;

                            if ((next == BYTE) && (length == 1))
                                modeList[1, i] = BYTE;
                        }
                    }

                    else
                    {
                        if (i == modeListCount - 1)
                        {
                            // Last block.
                            if ((last == TEXT) && (length < 7))
                                modeList[1, i] = TEXT;

                            if ((last == BYTE) && (length == 1))
                                modeList[1, i] = BYTE;
                        }

                        else
                        {
                            // Not first or last block.
                            if (((last == BYTE) && (next == BYTE)) && (length < 4))
                                modeList[1, i] = BYTE;

                            if (((last == BYTE) && (next == TEXT)) && (length < 4))
                                modeList[1, i] = TEXT;

                            if (((last == TEXT) && (next == BYTE)) && (length < 5))
                                modeList[1, i] = TEXT;

                            if (((last == TEXT) && (next == TEXT)) && (length < 8))
                                modeList[1, i] = TEXT;
                        }
                    }
                }
            }

            CombineModeBlocks(ref modeListCount, modeList);

            for (int i = 0; i < modeListCount; i++)
            {
                current = modeList[1, i];
                length = modeList[0, i];
                if (i != 0)
                    last = modeList[1, i - 1];

                else
                    last = 0;

                if (i != modeListCount - 1)
                    next = modeList[1, i + 1];

                else
                    next = 0;

                if ((current == TEXT) && (i > 0))
                {
                    // Not the first.
                    if (i == modeListCount - 1)
                    {
                        // The last one.
                        if ((last == BYTE) && (length == 1))
                            modeList[1, i] = BYTE;
                    }

                    else
                    {
                        // Not the last one.
                        if (((last == BYTE) && (next == BYTE)) && (length < 5))
                            modeList[1, i] = BYTE;

                        if ((((last == BYTE) && (next != BYTE)) || ((last != BYTE) && (next == BYTE))) && (length < 3))
                            modeList[1, i] = BYTE;
                    }
                }
            }

            CombineModeBlocks(ref modeListCount, modeList);
        }

        private void ProcessText(List<int> dataStream, int start, int length)
        {
            int modeListIndex, currentTable;
            char asciiValue;
            int[] listStream = new int[5000];
            int[,] listTable = new int[2, length];

            // listTable will contain the table numbers and the value of each character.
            for (modeListIndex = 0; modeListIndex < length; modeListIndex++)
            {
                asciiValue = barcodeData[start + modeListIndex];
                switch (asciiValue)
                {
                    case '\t':
                        listTable[0, modeListIndex] = 12;
                        listTable[1, modeListIndex] = 12;
                        break;

                    case '\n':
                        listTable[0, modeListIndex] = 8;
                        listTable[1, modeListIndex] = 15;
                        break;

                    case (char)13:
                        listTable[0, modeListIndex] = 12;
                        listTable[1, modeListIndex] = 11;
                        break;

                    default:
                        listTable[0, modeListIndex] = PDF417Tables.ASCIIXTable[asciiValue - 32];
                        listTable[1, modeListIndex] = PDF417Tables.ASCIIYTable[asciiValue - 32];
                        break;
                }
            }

            int index = 0;
            currentTable = 1; // Default table.
            for (int j = 0; j < length; j++)
            {
                if ((listTable[0, j] & currentTable) != 0)
                {
                    // The character is in the current table.
                    listStream[index] = listTable[1, j];
                    index++;
                }

                else
                {
                    // Obliged to change table.
                    bool flag = false; // True if we change table for only one character.
                    if (j == (length - 1))
                        flag = true;

                    else
                    {
                        if (!((listTable[0, j] & listTable[0, j + 1]) != 0))
                            flag = true;
                    }

                    if (flag)
                    {
                        // Change only one character - look for temporary switch.
                        if (((listTable[0, j] & 1) != 0) && (currentTable == 2))
                        {
                            // T_UPP 
                            listStream[index] = 27;
                            listStream[index + 1] = listTable[1, j];
                            index += 2;
                        }

                        if ((listTable[0, j] & 8) != 0)
                        {
                            // T_PUN 
                            listStream[index] = 29;
                            listStream[index + 1] = listTable[1, j];
                            index += 2;
                        }

                        if (!((((listTable[0, j] & 1) != 0) && (currentTable == 2)) || (listTable[0, j] & 8) != 0))
                        {
                            // No temporary switch available 
                            flag = false;
                        }
                    }

                    if (!flag)
                    {
                        int newTable;

                        if (j == (length - 1))
                            newTable = listTable[0, j];

                        else
                        {
                            if (!(((listTable[0, j] & listTable[0, j + 1])) != 0))
                                newTable = listTable[0, j];

                            else
                                newTable = listTable[0, j] & listTable[0, j + 1];
                        }

                        // Maintain the first if several tables are possible.
                        switch (newTable)
                        {
                            case 3:
                            case 5:
                            case 7:
                            case 9:
                            case 11:
                            case 13:
                            case 15:
                                newTable = 1;
                                break;

                            case 6:
                            case 10:
                            case 14:
                                newTable = 2;
                                break;

                            case 12:
                                newTable = 4;
                                break;
                        }

                        // Select the switch.
                        switch (currentTable)
                        {
                            case 1:
                                switch (newTable)
                                {
                                    case 2: listStream[index] = 27;
                                        index++;
                                        break;

                                    case 4:
                                        listStream[index] = 28;
                                        index++;
                                        break;

                                    case 8:
                                        listStream[index] = 28;
                                        index++;
                                        listStream[index] = 25;
                                        index++;
                                        break;
                                }
                                break;

                            case 2:
                                switch (newTable)
                                {
                                    case 1:
                                        listStream[index] = 28;
                                        index++;
                                        listStream[index] = 28;
                                        index++;
                                        break;

                                    case 4:
                                        listStream[index] = 28;
                                        index++;
                                        break;

                                    case 8:
                                        listStream[index] = 28;
                                        index++;
                                        listStream[index] = 25;
                                        index++;
                                        break;
                                }
                                break;

                            case 4:
                                switch (newTable)
                                {
                                    case 1:
                                        listStream[index] = 28;
                                        index++;
                                        break;

                                    case 2:
                                        listStream[index] = 27;
                                        index++;
                                        break;

                                    case 8:
                                        listStream[index] = 25;
                                        index++;
                                        break;
                                }
                                break;

                            case 8:
                                switch (newTable)
                                {
                                    case 1:
                                        listStream[index] = 29;
                                        index++;
                                        break;

                                    case 2:
                                        listStream[index] = 29;
                                        index++;
                                        listStream[index] = 27;
                                        index++;
                                        break;

                                    case 4:
                                        listStream[index] = 29;
                                        index++;
                                        listStream[index] = 28;
                                        index++;
                                        break;
                                }
                                break;
                        }

                        currentTable = newTable;
                        listStream[index] = listTable[1, j];
                        index++;
                    }
                }
            }

            if ((index & 1) != 0)
            {
                listStream[index] = 29;
                index++;
            }

            // Now translate the string listStream into codewords.
            dataStream.Add(TEXT);

            for (int j = 0; j < index; j += 2)
            {
                int codewordValue = (30 * listStream[j]) + listStream[j + 1];
                dataStream.Add(codewordValue);
            }
        }

        private void ProcessByte(List<int> dataStream, int start, int modeCount)
        {
            int length = 0;
            int chunkLength = 0;
            ulong mantisa = 0;
            ulong total = 0;

            if (modeCount == 1)
            {
                dataStream.Add(913);
                dataStream.Add((int)barcodeData[start]);
            }

            else
            {
                // Select the switch for multiple of 6 bytes.
                if (modeCount % 6 == 0)
                    dataStream.Add(924);

                else
                    dataStream.Add(BYTE);

                while (length < modeCount)
                {
                    chunkLength = modeCount - length;
                    if (chunkLength >= 6)  // Take groups of 6.
                    {
                        chunkLength = 6;
                        length += chunkLength;
                        total = 0;

                        while (chunkLength-- != 0)
                        {
                            mantisa = barcodeData[start++];
                            total |= mantisa << (chunkLength * 8);
                        }

                        chunkLength = 5;
                        int index = dataStream.Count;

                        while (chunkLength-- != 0)
                        {
                            dataStream.Add((int)(total % 900));
                            total /= 900;
                        }

                        dataStream.Reverse(index, 5);
                    }

                    else	// If it remains a group of less than 6 bytes.
                    {
                        length += chunkLength;
                        while (chunkLength-- != 0)
                            dataStream.Add(barcodeData[start++]);
                    }
                }
            }
        }

        void ProcessNumber(List<int> dataStream, int start, int modeCount)
        {
            int maxLength, divisor, number;
            List<int> buffer = new List<int>();
            StringBuilder moduloStream;
            StringBuilder multiplyStream;

            dataStream.Add(NUMBER);
            int j = 0;
            while (j < modeCount)
            {
                moduloStream = new StringBuilder();
                maxLength = modeCount - j;
                if (maxLength > 44)
                    maxLength = 44;

                moduloStream.Append("1");
                for (int i = 1; i <= maxLength; i++)
                    moduloStream.Append(barcodeData[start + i + j - 1]);

                do
                {
                    divisor = 900;
                    number = 0;
                    multiplyStream = new StringBuilder();

                    while (moduloStream.Length != 0)
                    {
                        number *= 10;
                        number += (int)(moduloStream[0] - '0');
                        moduloStream.Remove(0, 1);

                        if (number < divisor)
                        {
                            if (multiplyStream.Length != 0)
                                multiplyStream.Append("0");
                        }

                        else
                            multiplyStream.Append((char)((number / divisor) + '0'));

                        number %= divisor;
                    }

                    divisor = number;
                    buffer.Insert(0, divisor);
                    moduloStream.Clear();
                    moduloStream.Append(multiplyStream.ToString());
                }
                while (multiplyStream.Length != 0);

                for (int i = 0; i < buffer.Count; i++)
                    dataStream.Add(buffer[i]);

                j += maxLength;
            }
        }
        /// <summary>
        /// Generate a Micro PDF barcode.
        /// </summary>
        private void MicroPDF417()
        {
            int inputLength = barcodeData.Length;
            int offset;
            int modeListCount = 0;

            List<int> dataStream = new List<int>();
            int[] eccStream;

            // Encoding starts out the same as PDF417, so use the same code.
            int[,] modeList = new int[2, inputLength];
            for (int i = 0; i < inputLength; i++)
            {
                int mode = GetMode(barcodeData[i]);
                if (i == 0)     // First character.
                {
                    modeList[1, modeListCount] = mode;
                    modeList[0, modeListCount]++;
                }

                else
                {
                    // Next character same mode.
                    if (mode == modeList[1, modeListCount])
                        modeList[0, modeListCount]++;

                    else
                    {
                        // Next character a different mode.
                        modeListCount++;
                        modeList[1, modeListCount] = mode;
                        modeList[0, modeListCount]++;
                    }
                }
            }

            modeListCount++;
            SmoothPDF(ref modeListCount, modeList);

            // Compress the data.
            int dataIndex = 0;
            int dataStreamLength;
            for (int i = 0; i < modeListCount; i++)
            {
                switch (modeList[1, i])
                {
                    case TEXT:
                        ProcessText(dataStream, dataIndex, modeList[0, i]);
                        break;

                    case BYTE:
                        ProcessByte(dataStream, dataIndex, modeList[0, i]);
                        break;

                    case NUMBER:
                        ProcessNumber(dataStream, dataIndex, modeList[0, i]);
                        break;
                }

                dataIndex += modeList[0, i];
            }

            //
            // This is where it all changes!
            //
            dataStreamLength = dataStream.Count;
            if (dataStreamLength > 126)
                throw new InvalidDataLengthException();

            if (optionDataColumns > 4)
                optionDataColumns = 0;

            // Now figure out which variant of the symbol to use and load values accordingly.
            int variant = 0;
            if ((optionDataColumns == 1) && (dataStreamLength > 20))
            {
                // The user specified 1 column but the data doesn't fit - go to automatic 
                optionDataColumns = 0;
            }

            if ((optionDataColumns == 2) && (dataStreamLength > 37))
            {
                // The user specified 2 columns but the data doesn't fit - go to automatic 
                optionDataColumns = 0;
            }

            if ((optionDataColumns == 3) && (dataStreamLength > 82))
            {
                // The user specified 3 columns but the data doesn't fit - go to automatic 
                optionDataColumns = 0;
            }

            if (optionDataColumns == 1)
            {
                variant = 6;
                if (dataStreamLength <= 16)
                    variant = 5;

                if (dataStreamLength <= 12)
                    variant = 4;

                if (dataStreamLength <= 10)
                    variant = 3;

                if (dataStreamLength <= 7)
                    variant = 2;

                if (dataStreamLength <= 4)
                    variant = 1;
            }

            if (optionDataColumns == 2)
            {
                variant = 13;
                if (dataStreamLength <= 33)
                    variant = 12;

                if (dataStreamLength <= 29)
                    variant = 11;

                if (dataStreamLength <= 24)
                    variant = 10;

                if (dataStreamLength <= 19)
                    variant = 9;

                if (dataStreamLength <= 13)
                    variant = 8;

                if (dataStreamLength <= 8)
                    variant = 7;
            }

            if (optionDataColumns == 3)
            {
                // The user specified 3 columns and the data does fit 
                variant = 23;
                if (dataStreamLength <= 70)
                    variant = 22;

                if (dataStreamLength <= 58)
                    variant = 21;

                if (dataStreamLength <= 46)
                    variant = 20;

                if (dataStreamLength <= 34)
                    variant = 19;

                if (dataStreamLength <= 24)
                    variant = 18;

                if (dataStreamLength <= 18)
                    variant = 17;

                if (dataStreamLength <= 14)
                    variant = 16;

                if (dataStreamLength <= 10)
                    variant = 15;

                if (dataStreamLength <= 6)
                    variant = 14;
            }

            if (optionDataColumns == 4)
            {
                // The user specified 4 columns and the data does fit. 
                variant = 34;
                if (dataStreamLength <= 108)
                    variant = 33;

                if (dataStreamLength <= 90)
                    variant = 32;

                if (dataStreamLength <= 72)
                    variant = 31;

                if (dataStreamLength <= 54)
                    variant = 30;

                if (dataStreamLength <= 39)
                    variant = 29;

                if (dataStreamLength <= 30)
                    variant = 28;

                if (dataStreamLength <= 24)
                    variant = 27;

                if (dataStreamLength <= 18)
                    variant = 26;

                if (dataStreamLength <= 12)
                    variant = 25;

                if (dataStreamLength <= 8)
                    variant = 24;
            }

            if (variant == 0)
            {
                // Let ZintNET choose automatically from all available variations. 
                for (int i = 27; i >= 0; i--)
                {
                    if (PDF417Tables.MicroAutoSize[i] >= dataStreamLength)
                        variant = PDF417Tables.MicroAutoSize[i + 28];
                }
            }

            // Now we have the variant we can load the data.
            variant--;
            optionDataColumns = PDF417Tables.MicroVariants[variant];            // columns 
            int rowCount = PDF417Tables.MicroVariants[variant + 34];            // row Count 
            int eccCodewords = PDF417Tables.MicroVariants[variant + 68];        // number of error correction CWs 
            int dataCodewords = (optionDataColumns * rowCount) - eccCodewords;  // number of data CWs 
            int padding = dataCodewords - dataStreamLength;                     // amount of padding required 
            offset = PDF417Tables.MicroVariants[variant + 102];                 // coefficient offset 

            // Add the padding 
            while (padding > 0)
            {
                dataStream.Add(900);
                padding--;
            }

            // Reed-Solomon error correction 
            dataStreamLength = dataStream.Count;
            eccStream = new int[eccCodewords];
            for (int i = 0; i < dataStreamLength; i++)
            {
                int total = (dataStream[i] + eccStream[eccCodewords - 1]) % 929;
                for (int j = eccCodewords - 1; j > 0; j--)
                    eccStream[j] = ((eccStream[j - 1] + 929) - (total * PDF417Tables.MicroCoefficients[offset + j]) % 929) % 929;

                eccStream[0] = (929 - (total * PDF417Tables.MicroCoefficients[offset]) % 929) % 929;
            }

            for (int j = 0; j < eccCodewords; j++)
            {
                if (eccStream[j] != 0)
                    eccStream[j] = 929 - eccStream[j];
            }

            // Add the reed-solomon codewords to the data stream. 
            for (int i = eccCodewords - 1; i >= 0; i--)
                dataStream.Add(eccStream[i]);

            // RAP (Row Address Pattern) start values 
            int leftRAPStart = PDF417Tables.RowAddressTable[variant];
            int centreRAPStart = PDF417Tables.RowAddressTable[variant + 34];
            int rightRAPStart = PDF417Tables.RowAddressTable[variant + 68];
            int startCluster = PDF417Tables.RowAddressTable[variant + 102] / 3;

            // Start encoding. 
            int leftRAP = leftRAPStart;
            int centreRAP = centreRAPStart;
            int rightRAP = rightRAPStart;
            int cluster = startCluster; // Cluster can be 0, 1 or 2 for Cluster(0), Cluster(3) and Cluster(6). 

            StringBuilder codeString = new StringBuilder();
            BitVector bitPattern = new BitVector();
            int[] buffer = new int[optionDataColumns + 1];
            for (int row = 0; row < rowCount; row++)
            {
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;

                for (int i = 0; i < optionDataColumns; i++)
                    buffer[i + 1] = dataStream[row * optionDataColumns + i];

                // Copy the data into code string.
                offset = 929 * cluster;
                codeString.Append(PDF417Tables.RowAddressPattern[leftRAP]);
                codeString.Append("1");
                codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[1]]);
                codeString.Append("1");

                if (optionDataColumns == 3)
                    codeString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (optionDataColumns >= 2)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[2]]);
                    codeString.Append("1");
                }

                if (optionDataColumns == 4)
                    codeString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (optionDataColumns >= 3)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[3]]);
                    codeString.Append("1");
                }

                if (optionDataColumns == 4)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[4]]);
                    codeString.Append("1");
                }

                codeString.Append(PDF417Tables.RowAddressPattern[rightRAP]);
                codeString.Append("1"); // Stop.

                // Now codeString is a mixture of letters and numbers 
                bool latch = true;
                int value;
                for (int i = 0; i < codeString.Length; i++)
                {
                    if ((codeString[i] >= '0') && (codeString[i] <= '9'))
                    {
                        value = (int)(codeString[i] - '0');
                        bitPattern.AppendBits((latch) ? 0xfff : 0, value);
                        latch = !latch;
                    }

                    else
                    {
                        int position = PDF417Tables.PDFSet.IndexOf(codeString[i]);
                        if (position >= 0 && position < 32)
                            bitPattern.AppendBits(position, 5);
                    }
                }

                int size = bitPattern.SizeInBits;
                byte[] rowData = new byte[size];
                for (int i = 0; i < size; i++)
                    rowData[i] = bitPattern[i];

                SymbolData symbolData = new SymbolData(rowData, 2);
                Symbol.Add(symbolData);

                codeString.Clear();
                bitPattern.Clear();

                // Set up RAPs and Cluster for next row.
                leftRAP++;
                centreRAP++;
                rightRAP++;
                cluster++;

                if (leftRAP == 53)
                    leftRAP = 1;

                if (centreRAP == 53)
                    centreRAP = 1;

                if (rightRAP == 53)
                    rightRAP = 1;

                if (cluster == 3)
                    cluster = 0;
            }
        }
    }
}