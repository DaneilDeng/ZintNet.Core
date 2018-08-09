/* CodeOneEncoder.cs - Handles encoding of Code One 2D symbol */

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
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Extensions;


namespace ZintNet.Core.Encoders
{
    /// <summary>
    /// Builds a Code1 Symbol
    /// </summary>
    internal class CodeOneEncoder : SymbolEncoder
    {
        # region Tables and Constants
        static readonly int[] C40Shift = {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3};

        static readonly int[] C40Value = {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
            15, 16, 17, 18, 19, 20, 21, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
            22, 23, 24, 25, 26, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31};

        static readonly int[] TextShift = {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            2, 2, 2, 2, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3};

        static readonly int[] TextValue = {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
            15, 16, 17, 18, 19, 20, 21, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26,
            22, 23, 24, 25, 26, 0, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 27, 28, 29, 30, 31};

        static readonly int[] Code1Height = {
            16, 22, 28, 40, 52, 70, 104, 148};

        static readonly int[] Code1Width = {
            18, 22, 32, 42, 54, 76, 98, 134};

        static readonly int[] Code1DataLength = {
            10, 19, 44, 91, 182, 370, 732, 1480};

        static readonly int[] Code1EccLength = {
            10, 16, 26, 44, 70, 140, 280, 560};

        static int[] Code1Blocks = {
            1, 1, 1, 1, 1, 2, 4, 8};

        static readonly int[] Code1DatBlocks = {
            10, 19, 44, 91, 182, 185, 183, 185};

        static readonly int[] Code1EccBlocks = {
            10, 16, 26, 44, 70, 70, 70, 70};

        static readonly int[] Code1GridWidth = {
            4, 5, 7, 9, 12, 17, 22, 30};

        static readonly int[] Code1GridHeight = {
            5, 7, 10, 15, 21, 30, 46, 68};

        private const int ASCII = 1;
        private const int C40 = 2;
        private const int DECIMAL = 3;
        private const int TEXT = 4;
        private const int EDIFACT = 5;
        private const int BYTE = 6;
        # endregion

        private int optionSymbolSize;
   
        public CodeOneEncoder(Symbology symbology, string barcodeMessage, int optionSymbolSize, EncodingMode mode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionSymbolSize = optionSymbolSize;
            this.encodingMode = mode;
            this.Symbol = new Collection<SymbolData>();
        }

        public override Collection<SymbolData> EncodeData()
        {
            this.Symbol = new Collection<SymbolData>();

            switch (encodingMode)
            {
                case EncodingMode.Standard:
                    isGS1 = false;
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    break;

                case EncodingMode.GS1:
                    isGS1 = true;
                    barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                    break;

                default:
                    return null;
            }

            CodeOne();
            return Symbol;
        }

        private void CodeOne()
        {
            int size = 1;
            int index;
            uint[] data;
            uint[] ecc;
            uint[] stream;
            byte[] symbolGrid;
            int symbolRows = 0;
            int symbolWidth = 0;
            char[,] dataGrid = new char[136, 120];
            int row, column;
            int subVersion = 0;

            int inputLength = barcodeData.Length;
            if ((optionSymbolSize < 0) || (optionSymbolSize > 10))
               throw new System.ArgumentException("Code One: Invalid Symbol size - " + optionSymbolSize);

            if (optionSymbolSize == 9)
            {
                // Version S.
                int codewords;
                int[] binaryInput;
                ulong inputValue;
                int blockWidth;

                data = new uint[15];
                ecc = new uint[15];
                stream = new uint[30];

                if (inputLength > 18)
                    throw new InvalidDataLengthException("Code One: Input data is too long.");

                for (int i = 0; i < inputLength; i++)
                {
                    if (!Char.IsDigit(barcodeData[i]))
                        throw new InvalidDataException("Code One: Version S encodes numeric input only.");
                }

                // Start with Version S-30.
                subVersion = 3;
                codewords = 12;
                blockWidth = 6;

                if (inputLength <= 12)
                {
                    // Version S-20.
                    subVersion = 2;
                    codewords = 8;
                    blockWidth = 4;
                }

                if (inputLength <= 6)
                {
                    // Version S-10.
                    subVersion = 1;
                    codewords = 4;
                    blockWidth = 2;
                }

                inputValue = ulong.Parse(barcodeMessage, CultureInfo.CurrentCulture);
                binaryInput = new int[codewords * 5];
                for (int i = 0; i < binaryInput.Length; i++)
                    binaryInput[i] = (int)((inputValue >> i) & 0x1);

                for (int i = 0; i < codewords; i++)
                {
                    data[codewords - i - 1] += (uint)(1 * binaryInput[(i * 5)]);
                    data[codewords - i - 1] += (uint)(2 * binaryInput[(i * 5) + 1]);
                    data[codewords - i - 1] += (uint)(4 * binaryInput[(i * 5) + 2]);
                    data[codewords - i - 1] += (uint)(8 * binaryInput[(i * 5) + 3]);
                    data[codewords - i - 1] += (uint)(16 * binaryInput[(i * 5) + 4]);
                }

                ReedSolomon.RSInitialise(0x25, codewords, 1);
                ReedSolomon.RSEncode(codewords, data, ecc);
                for (int i = 0; i < codewords; i++)
                {
                    stream[i] = data[i];
                    stream[i + codewords] = ecc[codewords - i - 1];
                }

                index = 0;
                for (row = 0; row < 2; row++)
                {
                    for (column = 0; column < blockWidth; column++)
                    {
                        if ((stream[index] & 0x10) > 0)
                            dataGrid[row * 2, column * 5] = '1';

                        if ((stream[index] & 0x08) > 0)
                            dataGrid[row * 2, (column * 5) + 1] = '1';

                        if ((stream[index] & 0x04) > 0)
                            dataGrid[row * 2, (column * 5) + 2] = '1';

                        if ((stream[index] & 0x02) > 0)
                            dataGrid[(row * 2) + 1, column * 5] = '1';

                        if ((stream[index] & 0x01) > 0)
                            dataGrid[(row * 2) + 1, (column * 5) + 1] = '1';

                        if ((stream[index + 1] & 0x10) > 0)
                            dataGrid[row * 2, (column * 5) + 3] = '1';

                        if ((stream[index + 1] & 0x08) > 0)
                            dataGrid[row * 2, (column * 5) + 4] = '1';

                        if ((stream[index + 1] & 0x04) > 0)
                            dataGrid[(row * 2) + 1, (column * 5) + 2] = '1';

                        if ((stream[index + 1] & 0x02) > 0)
                            dataGrid[(row * 2) + 1, (column * 5) + 3] = '1';

                        if ((stream[index + 1] & 0x01) > 0)
                            dataGrid[(row * 2) + 1, (column * 5) + 4] = '1';

                        index += 2;
                    }
                }

                size = 9;
                symbolRows = 8;
                symbolWidth = 10 * subVersion + 1;
            }

            if (optionSymbolSize == 10)
            {
                // Version T.
                data = new uint[40];
                ecc = new uint[25];
                stream = new uint[65];
                int dataLength;
                int dataCodeWord, eccCodeWord, blockWidth;

                for (int i = 0; i < 40; i++)
                    data[i] = 0;

                dataLength = Code1Encode(data, inputLength);

                if (dataLength == 0)
                    throw new InvalidDataLengthException("Code One: Input data is too long for symbol size.");

                if (dataLength > 38)
                    throw new InvalidDataLengthException("Code One: Input data is too long.");

                size = 10;
                subVersion = 3;
                dataCodeWord = 38;
                eccCodeWord = 22;
                blockWidth = 12;
                if (dataLength <= 24)
                {
                    subVersion = 2;
                    dataCodeWord = 24;
                    eccCodeWord = 16;
                    blockWidth = 8;
                }

                if (dataLength <= 10)
                {
                    subVersion = 1;
                    dataCodeWord = 10;
                    eccCodeWord = 10;
                    blockWidth = 4;
                }

                for (int i = dataLength; i < dataCodeWord; i++)
                    data[i] = 129;  // Add padding.

                // Calculate error correction data.
                ReedSolomon.RSInitialise(0x12d, eccCodeWord, 1);
                ReedSolomon.RSEncode(dataCodeWord, data, ecc);

                // "stream" combines data and error correction data.
                for (int i = 0; i < dataCodeWord; i++)
                    stream[i] = data[i];

                for (int i = 0; i < eccCodeWord; i++)
                    stream[dataCodeWord + i] = ecc[eccCodeWord - i - 1];

                index = 0;
                for (row = 0; row < 5; row++)
                {
                    for (column = 0; column < blockWidth; column++)
                    {
                        if ((stream[index] & 0x80) > 0)
                            dataGrid[row * 2, column * 4] = '1';

                        if ((stream[index] & 0x40) > 0)
                            dataGrid[row * 2, (column * 4) + 1] = '1';

                        if ((stream[index] & 0x20) > 0)
                            dataGrid[row * 2, (column * 4) + 2] = '1';

                        if ((stream[index] & 0x10) > 0)
                            dataGrid[row * 2, (column * 4) + 3] = '1';

                        if ((stream[index] & 0x08) > 0)
                            dataGrid[(row * 2) + 1, column * 4] = '1';

                        if ((stream[index] & 0x04) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 1] = '1';

                        if ((stream[index] & 0x02) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 2] = '1';

                        if ((stream[index] & 0x01) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 3] = '1';

                        index++;
                    }
                }

                symbolRows = 16;
                symbolWidth = (subVersion * 16) + 1;
            }

            if ((optionSymbolSize != 9) && (optionSymbolSize != 10))
            {
                // Version A to H.
                uint[] subData = new uint[190];
                uint[] subEcc = new uint[75];
                data = new uint[1500];
                ecc = new uint[600];
                stream = new uint[2100];
                int dataBlockCount;
                int dataLength;

                dataLength = Code1Encode(data, inputLength);
                if (dataLength == 0)
                    throw new InvalidDataLengthException("Code1: Input data is too long for symbol size.");

                for (int i = 7; i >= 0; i--)
                {
                    if (Code1DataLength[i] >= dataLength)
                        size = i + 1;
                }

                if (optionSymbolSize > size)
                    size = optionSymbolSize;

                for (int i = dataLength; i < Code1DataLength[size - 1]; i++)
                    data[i] = 129; /* Pad */

                // Calculate error correction data.
                dataLength = Code1DataLength[size - 1];
                dataBlockCount = Code1Blocks[size - 1];
                ReedSolomon.RSInitialise(0x12d, Code1EccBlocks[size - 1], 0);

                for (int i = 0; i < dataBlockCount; i++)
                {
                    for (int j = 0; j < Code1DatBlocks[size - 1]; j++)
                        subData[j] = data[j * dataBlockCount + i];

                    ReedSolomon.RSEncode(Code1DatBlocks[size - 1], subData, subEcc);
                    for (int j = 0; j < Code1EccBlocks[size - 1]; j++)
                        ecc[Code1EccLength[size - 1] - (j * dataBlockCount + i) - 1] = subEcc[j];
                }

                // "stream" combines data and error correction data.
                for (int i = 0; i < dataLength; i++)
                    stream[i] = data[i];

                for (int i = 0; i < Code1EccLength[size - 1]; i++)
                    stream[dataLength + i] = ecc[i];

                index = 0;
                for (row = 0; row < Code1GridHeight[size - 1]; row++)
                {
                    for (column = 0; column < Code1GridWidth[size - 1]; column++)
                    {
                        if ((stream[index] & 0x80) > 0)
                            dataGrid[row * 2, column * 4] = '1';

                        if ((stream[index] & 0x40) > 0)
                            dataGrid[row * 2, (column * 4) + 1] = '1';

                        if ((stream[index] & 0x20) > 0)
                            dataGrid[row * 2, (column * 4) + 2] = '1';

                        if ((stream[index] & 0x10) > 0)
                            dataGrid[row * 2, (column * 4) + 3] = '1';

                        if ((stream[index] & 0x08) > 0)
                            dataGrid[(row * 2) + 1, column * 4] = '1';

                        if ((stream[index] & 0x04) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 1] = '1';

                        if ((stream[index] & 0x02) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 2] = '1';

                        if ((stream[index] & 0x01) > 0)
                            dataGrid[(row * 2) + 1, (column * 4) + 3] = '1';

                        index++;
                    }
                }

                symbolRows = Code1Height[size - 1];
                symbolWidth = Code1Width[size - 1];
            }

            symbolGrid = new byte[symbolRows * symbolWidth];

            switch (size)
            {
                case 1: // Version A.
                    CentralFinder(symbolGrid, symbolWidth, 6, 3, 1);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 6, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 12, 5, false);
                    SetGridModule(symbolGrid, symbolWidth, 5, 12, 1);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 15);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 5, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 5, 12, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 5, 0, 5, 12, 6, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 5, 12, 5, 4, 6, 2);
                    break;

                case 2: // Version B.
                    CentralFinder(symbolGrid, symbolWidth, 8, 4, 1);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 8, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 16, 7, false);
                    SetGridModule(symbolGrid, symbolWidth, 7, 16, 1);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 21);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 7, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 7, 16, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 7, 0, 7, 16, 8, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 7, 16, 7, 4, 8, 2);
                    break;

                case 3: // Version C.
                    CentralFinder(symbolGrid, symbolWidth, 11, 4, 2);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 11, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 13, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 10, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 10, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 27);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 10, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 10, 20, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 24, 10, 4, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 10, 0, 10, 4, 8, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 10, 4, 10, 20, 8, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 10, 24, 10, 4, 8, 4);
                    break;

                case 4: // Version D.
                    CentralFinder(symbolGrid, symbolWidth, 16, 5, 1);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 16, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 20, 16, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 36, 16, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 15, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 20, 15, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 36, 15, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 12);
                    Spigot(symbolGrid, symbolWidth, 27);
                    Spigot(symbolGrid, symbolWidth, 39);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 15, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 15, 14, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 18, 15, 14, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 32, 15, 4, 0, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 15, 0, 15, 4, 10, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 15, 4, 15, 14, 10, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 15, 18, 15, 14, 10, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 15, 32, 15, 4, 10, 6);
                    break;

                case 5: // Version E.
                    CentralFinder(symbolGrid, symbolWidth, 22, 5, 2);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 22, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 24, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 22, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 21, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 21, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 21, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 12);
                    Spigot(symbolGrid, symbolWidth, 39);
                    Spigot(symbolGrid, symbolWidth, 51);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 21, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 21, 20, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 24, 21, 20, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 44, 21, 4, 0, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 21, 0, 21, 4, 10, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 21, 4, 21, 20, 10, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 21, 24, 21, 20, 10, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 21, 44, 21, 4, 10, 6);
                    break;

                case 6: // Version F.
                    CentralFinder(symbolGrid, symbolWidth, 31, 5, 3);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 31, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 35, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 31, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 70, 35, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 4, 30, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 30, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 30, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 70, 30, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 12);
                    Spigot(symbolGrid, symbolWidth, 24);
                    Spigot(symbolGrid, symbolWidth, 45);
                    Spigot(symbolGrid, symbolWidth, 57);
                    Spigot(symbolGrid, symbolWidth, 69);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 30, 4, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 4, 30, 20, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 24, 30, 20, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 44, 30, 20, 0, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 64, 30, 4, 0, 8);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 30, 0, 30, 4, 10, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 30, 4, 30, 20, 10, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 30, 24, 30, 20, 10, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 30, 44, 30, 20, 10, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 30, 64, 30, 4, 10, 8);
                    break;

                case 7: // Version G.
                    CentralFinder(symbolGrid, symbolWidth, 47, 6, 2);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 6, 47, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 27, 49, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 47, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 69, 49, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 90, 47, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 6, 46, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 27, 46, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 48, 46, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 69, 46, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 90, 46, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 12);
                    Spigot(symbolGrid, symbolWidth, 24);
                    Spigot(symbolGrid, symbolWidth, 36);
                    Spigot(symbolGrid, symbolWidth, 67);
                    Spigot(symbolGrid, symbolWidth, 79);
                    Spigot(symbolGrid, symbolWidth, 91);
                    Spigot(symbolGrid, symbolWidth, 103);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 46, 6, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 6, 46, 19, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 25, 46, 19, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 44, 46, 19, 0, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 63, 46, 19, 0, 8);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 82, 46, 6, 0, 10);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 0, 46, 6, 12, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 6, 46, 19, 12, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 25, 46, 19, 12, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 44, 46, 19, 12, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 63, 46, 19, 12, 8);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 46, 82, 46, 6, 12, 10);
                    break;

                case 8: // Version H.
                    CentralFinder(symbolGrid, symbolWidth, 69, 6, 3);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 6, 69, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 73, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 46, 69, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 66, 73, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 86, 69, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 106, 73, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 126, 69, true);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 6, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 26, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 46, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 66, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 86, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 106, 68, false);
                    Verticle(symbolGrid, symbolRows, symbolWidth, 126, 68, false);
                    Spigot(symbolGrid, symbolWidth, 0);
                    Spigot(symbolGrid, symbolWidth, 12);
                    Spigot(symbolGrid, symbolWidth, 24);
                    Spigot(symbolGrid, symbolWidth, 36);
                    Spigot(symbolGrid, symbolWidth, 48);
                    Spigot(symbolGrid, symbolWidth, 60);
                    Spigot(symbolGrid, symbolWidth, 87);
                    Spigot(symbolGrid, symbolWidth, 99);
                    Spigot(symbolGrid, symbolWidth, 111);
                    Spigot(symbolGrid, symbolWidth, 123);
                    Spigot(symbolGrid, symbolWidth, 135);
                    Spigot(symbolGrid, symbolWidth, 147);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 68, 6, 0, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 6, 68, 18, 0, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 24, 68, 18, 0, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 42, 68, 18, 0, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 60, 68, 18, 0, 8);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 78, 68, 18, 0, 10);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 96, 68, 18, 0, 12);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 114, 68, 6, 0, 14);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 0, 68, 6, 12, 0);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 6, 68, 18, 12, 2);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 24, 68, 18, 12, 4);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 42, 68, 18, 12, 6);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 60, 68, 18, 12, 8);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 78, 68, 18, 12, 10);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 96, 68, 18, 12, 12);
                    BlockCopy(symbolGrid, symbolWidth, dataGrid, 68, 114, 68, 6, 12, 14);
                    break;

                case 9: // Version S.
                    Horizontal(symbolGrid, symbolWidth, 5, true);
                    Horizontal(symbolGrid, symbolWidth, 7, true);
                    SetGridModule(symbolGrid, symbolWidth, 6, 0, 1);
                    SetGridModule(symbolGrid, symbolWidth, 6, symbolWidth - 1, 1);
                    SetGridModule(symbolGrid, symbolWidth, 7, 1, 0);
                    SetGridModule(symbolGrid, symbolWidth, 7, symbolWidth - 2, 0);
                    switch (subVersion)
                    {
                        case 1: // Version S-10.
                            //set_module(0, 5);
                            SetGridModule(symbolGrid, symbolWidth, 0, 5, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 4, 5, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 5, 4, 5, 0, 1);
                            break;

                        case 2: /* Version S-20 */
                            //set_module(0, 10);
                            SetGridModule(symbolGrid, symbolWidth, 0, 10, 1);
                            //set_module(4, 10);
                            SetGridModule(symbolGrid, symbolWidth, 4, 10, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 4, 10, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 10, 4, 10, 0, 1);
                            break;

                        case 3: /* Version S-30 */
                            SetGridModule(symbolGrid, symbolWidth, 0, 15, 1);
                            SetGridModule(symbolGrid, symbolWidth, 4, 15, 1);
                            SetGridModule(symbolGrid, symbolWidth, 6, 15, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 4, 15, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 15, 4, 15, 0, 1);
                            break;
                    }
                    break;

                case 10: // Version T.
                    Horizontal(symbolGrid, symbolWidth, 11, true);
                    Horizontal(symbolGrid, symbolWidth, 13, true);
                    Horizontal(symbolGrid, symbolWidth, 15, true);
                    SetGridModule(symbolGrid, symbolWidth, 12, 0, 1);
                    SetGridModule(symbolGrid, symbolWidth, 12, symbolWidth - 1, 1);
                    SetGridModule(symbolGrid, symbolWidth, 14, 0, 1);
                    SetGridModule(symbolGrid, symbolWidth, 14, symbolWidth - 1, 1);
                    SetGridModule(symbolGrid, symbolWidth, 13, 1, 0);
                    SetGridModule(symbolGrid, symbolWidth, 13, symbolWidth - 2, 0);
                    SetGridModule(symbolGrid, symbolWidth, 15, 1, 0);
                    SetGridModule(symbolGrid, symbolWidth, 15, symbolWidth - 2, 0);
                    switch (subVersion)
                    {
                        case 1: // Version T-16.
                            SetGridModule(symbolGrid, symbolWidth, 0, 8, 1);
                            SetGridModule(symbolGrid, symbolWidth, 10, 8, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 10, 8, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 8, 10, 8, 0, 1);
                            break;

                        case 2: // Version T-32.
                            SetGridModule(symbolGrid, symbolWidth, 0, 16, 1);
                            SetGridModule(symbolGrid, symbolWidth, 10, 16, 1);
                            SetGridModule(symbolGrid, symbolWidth, 12, 16, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 10, 16, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 16, 10, 16, 0, 1);
                            break;

                        case 3: // Verion T-48.
                            SetGridModule(symbolGrid, symbolWidth, 0, 24, 1);
                            SetGridModule(symbolGrid, symbolWidth, 10, 24, 1);
                            SetGridModule(symbolGrid, symbolWidth, 12, 24, 1);
                            SetGridModule(symbolGrid, symbolWidth, 14, 24, 1);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 0, 10, 24, 0, 0);
                            BlockCopy(symbolGrid, symbolWidth, dataGrid, 0, 24, 10, 24, 0, 1);
                            break;
                    }
                    break;
            }

            // Encode the data.
            byte[] rowData;
            for (int y = 0; y < symbolRows; y++)
            {
                rowData = new byte[symbolWidth];
                for (int x = 0; x < symbolWidth; x++)
                    rowData[x] = symbolGrid[(y * symbolWidth) + x];

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Add(symbolData);
            }

        }

        int Code1Encode(uint[] target, int inputLength)
        {
            int currentMode, nextMode;
            int sourceIndex, targetIndex, j;
            bool latch, done;
            int[] c40Buffer = new int[6];
            int c40Index;
            int[] textBuffer = new int[6];
            int textIndex;
            int[] edifactBuffer = new int[6];
            int edifactIndex;
            BitVector decimalBinary = new BitVector();
            BitVector tempBinary;
            byte[] binaryBytes;
            int byteStart = 0;

            sourceIndex = 0;
            targetIndex = 0;
            latch = false;
            done = false;
            c40Index = 0;
            textIndex = 0;
            edifactIndex = 0;

            if (isGS1)
            {
                target[targetIndex] = 232;   // FNC1.
                targetIndex++;
            }

            // Step A.
            currentMode = ASCII;
            nextMode = ASCII;

            do
            {
                if (currentMode != nextMode)
                {
                    // Change mode.
                    switch (nextMode)
                    {
                        case C40: target[targetIndex] = 230;
                            targetIndex++;
                            break;

                        case TEXT: target[targetIndex] = 239;
                            targetIndex++;
                            break;

                        case EDIFACT: target[targetIndex] = 238;
                            targetIndex++;
                            break;

                        case BYTE: target[targetIndex] = 231;
                            targetIndex++;
                            break;
                    }
                }

                if ((currentMode != BYTE) && (nextMode == BYTE))
                    byteStart = targetIndex;

                currentMode = nextMode;

                if (currentMode == ASCII)
                {
                    // Step B - ASCII encodation.
                    nextMode = ASCII;

                    if ((inputLength - sourceIndex) >= 21)
                    {
                        // Step B1.
                        j = 0;

                        for (int i = 0; i < 21; i++)
                        {
                            if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                j++;
                        }

                        if (j == 21)
                        {
                            nextMode = DECIMAL;
                            decimalBinary.AppendBits(15, 4);    // "1111"
                        }
                    }

                    if ((nextMode == ASCII) && ((inputLength - sourceIndex) >= 13))
                    {
                        // Step B2.
                        j = 0;

                        for (int i = 0; i < 13; i++)
                        {
                            if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                j++;
                        }

                        if (j == 13)
                        {
                            latch = false;
                            for (int i = sourceIndex + 13; i < inputLength; i++)
                            {
                                if (!((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9')))
                                    latch = true;
                            }

                            if (!latch)
                            {
                                nextMode = DECIMAL;
                                decimalBinary.AppendBits(15, 4);   // "1111"
                            }
                        }
                    }

                    if (nextMode == ASCII)
                    {
                        // Step B3.
                        if (sourceIndex + 1 < inputLength && Char.IsDigit(barcodeData[sourceIndex]) && Char.IsDigit(barcodeData[sourceIndex + 1]))
                        {
                            target[targetIndex] = (uint)((10 * (barcodeData[sourceIndex] - '0')) + (barcodeData[sourceIndex + 1] - '0') + 130);
                            targetIndex++;
                            sourceIndex += 2;
                        }

                        else
                        {
                            if (isGS1 && (barcodeData[sourceIndex] == '['))
                            {
                                if ((inputLength - sourceIndex) >= 15)
                                {
                                    // Step B4.
                                    j = 0;

                                    for (int i = 0; i < 15; i++)
                                    {
                                        if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                            j++;
                                    }

                                    if (j == 15)
                                    {
                                        target[targetIndex] = 236; // FNC1 and change to Decimal Mode.
                                        targetIndex++;
                                        sourceIndex++;
                                        nextMode = DECIMAL;
                                    }
                                }

                                if ((inputLength - sourceIndex) >= 7)
                                {
                                    // Step B5.
                                    j = 0;

                                    for (int i = 0; i < 7; i++)
                                    {
                                        if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                            j++;
                                    }

                                    if (j == 7)
                                    {
                                        latch = false;
                                        for (int i = sourceIndex + 7; i < inputLength; i++)
                                        {
                                            if (!((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9')))
                                                latch = true;
                                        }

                                        if (!latch)
                                        {
                                            target[targetIndex] = 236; // FNC1 and change to Decimal Mode.
                                            targetIndex++;
                                            sourceIndex++;
                                            nextMode = DECIMAL;
                                        }
                                    }
                                }
                            }

                            if (nextMode == ASCII)
                            {
                                // Step B6.
                                nextMode = LookAheadTest(sourceIndex, inputLength, currentMode);

                                if (nextMode == ASCII)
                                {
                                    if (barcodeData[sourceIndex] > 127)
                                    {
                                        // Step B7.
                                        target[targetIndex] = 235; // FNC4.
                                        targetIndex++;
                                        target[targetIndex] = (uint)(barcodeData[sourceIndex] - 128) + 1;
                                        targetIndex++;
                                        sourceIndex++;
                                    }

                                    else
                                    {
                                        // Step B8.
                                        if (isGS1 && (barcodeData[sourceIndex] == '['))
                                        {
                                            target[targetIndex] = 232; // FNC1.
                                            targetIndex++;
                                            sourceIndex++;
                                        }

                                        else
                                        {
                                            target[targetIndex] = (uint)barcodeData[sourceIndex] + 1;
                                            targetIndex++;
                                            sourceIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (currentMode == C40)
                {
                    // Step C - C40 encodation.
                    int shift_set, value;

                    latch = false;
                    done = false;
                    nextMode = C40;
                    if (c40Index == 0)
                    {
                        if ((inputLength - sourceIndex) >= 12)
                        {
                            j = 0;
                            for (int i = 0; i < 12; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if (j == 12)
                            {
                                nextMode = ASCII;
                                done = true;
                            }
                        }

                        if ((inputLength - sourceIndex) >= 8)
                        {
                            j = 0;
                            for (int i = 0; i < 8; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if ((inputLength - sourceIndex) == 8)
                            {
                                latch = true;
                            }

                            else
                            {
                                latch = true;
                                for (j = sourceIndex + 8; j < inputLength; j++)
                                {
                                    if ((barcodeData[j] <= '0') || (barcodeData[j] >= '9'))
                                        latch = false;
                                }
                            }

                            if ((j == 8) && latch)
                            {
                                nextMode = ASCII;
                                done = true;
                            }
                        }

                        if (!done)
                            nextMode = LookAheadTest(sourceIndex, inputLength, currentMode);
                    }

                    if (nextMode != C40)
                    {
                        target[targetIndex] = 255; // Unlatch.
                        targetIndex++;
                    }

                    else
                    {
                        if (barcodeData[sourceIndex] > 127)
                        {
                            c40Buffer[c40Index] = 1;
                            c40Index++;
                            c40Buffer[c40Index] = 30; // Upper Shift.
                            c40Index++;
                            shift_set = C40Shift[barcodeData[sourceIndex] - 128];
                            value = C40Value[barcodeData[sourceIndex] - 128];
                        }

                        else
                        {
                            shift_set = C40Shift[barcodeData[sourceIndex]];
                            value = C40Value[barcodeData[sourceIndex]];
                        }

                        if (isGS1 && (barcodeData[sourceIndex] == '['))
                        {
                            shift_set = 2;
                            value = 27; // FNC1.
                        }

                        if (shift_set != 0)
                        {
                            c40Buffer[c40Index] = shift_set - 1;
                            c40Index++;
                        }

                        c40Buffer[c40Index] = value;
                        c40Index++;

                        if (c40Index >= 3)
                        {
                            int iv;

                            iv = (1600 * c40Buffer[0]) + (40 * c40Buffer[1]) + (c40Buffer[2]) + 1;
                            target[targetIndex] = (uint)(iv / 256);
                            targetIndex++;
                            target[targetIndex] = (uint)(iv % 256);
                            targetIndex++;

                            c40Buffer[0] = c40Buffer[3];
                            c40Buffer[1] = c40Buffer[4];
                            c40Buffer[2] = c40Buffer[5];
                            c40Buffer[3] = 0;
                            c40Buffer[4] = 0;
                            c40Buffer[5] = 0;
                            c40Index -= 3;
                        }
                        sourceIndex++;
                    }
                }

                if (currentMode == TEXT)
                {
                    // Step D - Text encodation.
                    int shift_set, value;

                    latch = false;
                    done = false;

                    nextMode = TEXT;
                    if (textIndex == 0)
                    {
                        if ((inputLength - sourceIndex) >= 12)
                        {
                            j = 0;

                            for (int i = 0; i < 12; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if (j == 12)
                            {
                                nextMode = ASCII;
                                done = true;
                            }
                        }

                        if ((inputLength - sourceIndex) >= 8)
                        {
                            j = 0;

                            for (int i = 0; i < 8; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if ((inputLength - sourceIndex) == 8)
                                latch = true;

                            else
                            {
                                latch = true;
                                for (j = sourceIndex + 8; j < inputLength; j++)
                                {
                                    if ((barcodeData[j] <= '0') || (barcodeData[j] >= '9'))
                                        latch = false;
                                }
                            }

                            if ((j == 8) && latch)
                            {
                                nextMode = ASCII;
                                done = true;
                            }
                        }

                        if (!done)
                            nextMode = LookAheadTest(sourceIndex, inputLength, currentMode);
                    }

                    if (nextMode != TEXT)
                    {
                        target[targetIndex] = 255;   // Unlatch.
                        targetIndex++; 
                    }

                    else
                    {
                        if (barcodeData[sourceIndex] > 127)
                        {
                            textBuffer[textIndex] = 1;
                            textIndex++;
                            textBuffer[textIndex] = 30;
                            textIndex++;    // Upper Shift.
                            shift_set = TextShift[barcodeData[sourceIndex] - 128];
                            value = TextValue[barcodeData[sourceIndex] - 128];
                        }

                        else
                        {
                            shift_set = TextShift[barcodeData[sourceIndex]];
                            value = TextValue[barcodeData[sourceIndex]];
                        }

                        if (isGS1 && (barcodeData[sourceIndex] == '['))
                        {
                            shift_set = 2;
                            value = 27; // FNC1.
                        }

                        if (shift_set != 0)
                        {
                            textBuffer[textIndex] = shift_set - 1;
                            textIndex++;
                        }

                        textBuffer[textIndex] = value;
                        textIndex++;

                        if (textIndex >= 3)
                        {
                            int iv;

                            iv = (1600 * textBuffer[0]) + (40 * textBuffer[1]) + (textBuffer[2]) + 1;
                            target[targetIndex] = (uint)(iv / 256);
                            targetIndex++;
                            target[targetIndex] = (uint)(iv % 256);
                            targetIndex++;

                            textBuffer[0] = textBuffer[3];
                            textBuffer[1] = textBuffer[4];
                            textBuffer[2] = textBuffer[5];
                            textBuffer[3] = 0;
                            textBuffer[4] = 0;
                            textBuffer[5] = 0;
                            textIndex -= 3;
                        }
                        sourceIndex++;
                    }
                }

                if (currentMode == EDIFACT)
                {
                    // Step E - EDI Encodation.
                    int value = 0;

                    latch = false;
                    nextMode = EDIFACT;
                    if (edifactIndex == 0)
                    {
                        if ((inputLength - sourceIndex) >= 12)
                        {
                            j = 0;
                            for (int i = 0; i < 12; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if (j == 12)
                                nextMode = ASCII;
                        }

                        if ((inputLength - sourceIndex) >= 8)
                        {
                            j = 0;
                            for (int i = 0; i < 8; i++)
                            {
                                if ((barcodeData[sourceIndex + i] >= '0') && (barcodeData[sourceIndex + i] <= '9'))
                                    j++;
                            }

                            if ((inputLength - sourceIndex) == 8)
                                latch = true;

                            else
                            {
                                latch = true;
                                for (j = sourceIndex + 8; j < inputLength; j++)
                                {
                                    if ((barcodeData[j] <= '0') || (barcodeData[j] >= '9'))
                                        latch = false;
                                }
                            }

                            if ((j == 8) && latch)
                                nextMode = ASCII;
                        }

                        if (!((IsEdifact(barcodeData[sourceIndex]) && IsEdifact(barcodeData[sourceIndex + 1])) && IsEdifact(barcodeData[sourceIndex + 2])))
                            nextMode = ASCII;
                    }

                    if (nextMode != EDIFACT)
                    {
                        target[targetIndex] = 255;   // Unlatch.
                        targetIndex++;
                    }

                    else
                    {
                        if (barcodeData[sourceIndex] == 13)
                            value = 0;

                        if (barcodeData[sourceIndex] == '*')
                            value = 1;

                        if (barcodeData[sourceIndex] == '>')
                            value = 2;

                        if (barcodeData[sourceIndex] == ' ')
                            value = 3;

                        if ((barcodeData[sourceIndex] >= '0') && (barcodeData[sourceIndex] <= '9'))
                            value = barcodeData[sourceIndex] - '0' + 4;

                        if ((barcodeData[sourceIndex] >= 'A') && (barcodeData[sourceIndex] <= 'Z'))
                            value = barcodeData[sourceIndex] - 'A' + 14;

                        edifactBuffer[edifactIndex] = value;
                        edifactIndex++;

                        if (edifactIndex >= 3)
                        {
                            int iv;

                            iv = (1600 * edifactBuffer[0]) + (40 * edifactBuffer[1]) + (edifactBuffer[2]) + 1;
                            target[targetIndex] = (uint)(iv / 256);
                            targetIndex++;
                            target[targetIndex] = (uint)(iv % 256);
                            targetIndex++;

                            edifactBuffer[0] = edifactBuffer[3];
                            edifactBuffer[1] = edifactBuffer[4];
                            edifactBuffer[2] = edifactBuffer[5];
                            edifactBuffer[3] = 0;
                            edifactBuffer[4] = 0;
                            edifactBuffer[5] = 0;
                            edifactIndex -= 3;
                        }
                        sourceIndex++;
                    }
                }

                if (currentMode == DECIMAL)
                {
                    // Step F - Decimal encodation.
                    int value, decimalCount, dataLeft;

                    nextMode = DECIMAL;
                    dataLeft = inputLength - sourceIndex;
                    decimalCount = 0;

                    if (dataLeft >= 1)
                    {
                        if ((barcodeData[sourceIndex] >= '0') && (barcodeData[sourceIndex] <= '9'))
                            decimalCount = 1;
                    }

                    if (dataLeft >= 2)
                    {
                        if ((decimalCount == 1) && ((barcodeData[sourceIndex + 1] >= '0') && (barcodeData[sourceIndex + 1] <= '9')))
                            decimalCount = 2;
                    }

                    if (dataLeft >= 3)
                    {
                        if ((decimalCount == 2) && ((barcodeData[sourceIndex + 2] >= '0') && (barcodeData[sourceIndex + 2] <= '9')))
                            decimalCount = 3;
                    }

                    if (decimalCount != 3)
                    {
                        int bitsLeftInByte, targetCount;
                        // Finish Decimal mode and go back to ASCII.
                        decimalBinary.AppendBits(63, 6);    // Unlatch. "111111"
                        targetCount = 3;
                        if (decimalBinary.SizeInBits <= 16)
                            targetCount = 2;

                        if (decimalBinary.SizeInBits <= 8)
                            targetCount = 1;

                        bitsLeftInByte = (8 * targetCount) - decimalBinary.SizeInBits;
                        if (bitsLeftInByte == 8)
                            bitsLeftInByte = 0;

                        if (bitsLeftInByte == 2)
                            decimalBinary.AppendBits(1, 2); // "01"

                        if ((bitsLeftInByte == 4) || (bitsLeftInByte == 6))
                        {
                            if (decimalCount >= 1)
                            {
                                int sub_value = (barcodeData[sourceIndex] - '0') + 1;

                                if ((sub_value & 0x08) > 0)
                                    decimalBinary.AppendBit(1);  // '1'

                                else
                                    decimalBinary.AppendBit(0);

                                if ((sub_value & 0x04) < 0)
                                    decimalBinary.AppendBit(1);

                                else
                                    decimalBinary.AppendBit(0);

                                if ((sub_value & 0x02) > 0)
                                    decimalBinary.AppendBit(1);

                                else
                                    decimalBinary.AppendBit(0);

                                if ((sub_value & 0x01) > 0)
                                    decimalBinary.AppendBit(1);

                                else
                                    decimalBinary.AppendBit(0);

                                sourceIndex++;
                            }

                            else
                                decimalBinary.AppendBits(15, 4);   // "1111"
                        }

                        if (bitsLeftInByte == 6)
                            decimalBinary.AppendBits(1, 2); // "01"

                        // Binary buffer is full - transfer to target.
                        binaryBytes = decimalBinary.ToByteArray();
                        if (targetCount >= 1)
                        {
                            target[targetIndex] = (uint)binaryBytes[0];
                            targetIndex++;
                        }

                        if (targetCount >= 2)
                        {
                            target[targetIndex] = (uint)binaryBytes[1];
                            targetIndex++;
                        }

                        if (targetCount == 3)
                        {
                            target[targetIndex] = (uint)binaryBytes[2];
                            targetIndex++;
                        }

                        nextMode = ASCII;
                    }

                    else
                    {
                        // There are three digits - convert the value to binary.
                        value = (100 * (barcodeData[sourceIndex] - '0')) + (10 * (barcodeData[sourceIndex + 1] - '0')) + (barcodeData[sourceIndex + 2] - '0') + 1;
                        for (int p = 0; p < 10; p++)
                        {
                            if ((value & (0x200 >> p)) > 0)
                                decimalBinary.AppendBit(1);

                            else
                                decimalBinary.AppendBit(0);
                        }

                        sourceIndex += 3;
                        if (decimalBinary.SizeInBits >= 24)
                        {
                            // Binary buffer is full - transfer to target.
                            binaryBytes = decimalBinary.ToByteArray();
                            target[targetIndex] = (uint)binaryBytes[0];
                            targetIndex++;
                            target[targetIndex] = (uint)binaryBytes[1];
                            targetIndex++;
                            target[targetIndex] = (uint)binaryBytes[2];
                            targetIndex++;

                            tempBinary = new BitVector();
                            if (decimalBinary.SizeInBits > 24)
                            {
                                for (int i = 0; i < (decimalBinary.SizeInBits - 24); i++)
                                    tempBinary.AppendBit(decimalBinary[i + 24]);
                            }

                            decimalBinary.Clear();
                            decimalBinary.AppendBitVector(tempBinary);
                        }
                    }
                }

                if (currentMode == BYTE)
                {
                    nextMode = BYTE;

                    if (isGS1 && (barcodeData[sourceIndex] == '['))
                    {
                        nextMode = ASCII;
                    }
                    else
                    {
                        if (barcodeData[sourceIndex] <= 127)
                        {
                            nextMode = LookAheadTest(sourceIndex, inputLength, currentMode);
                        }
                    }

                    if (nextMode != BYTE)
                    {
                        // Insert byte field inputLength.
                        if ((targetIndex - byteStart) <= 249)
                        {
                            for (int i = targetIndex; i >= byteStart; i--)
                                target[i + 1] = target[i];

                            target[byteStart] = (uint)(targetIndex - byteStart);
                            targetIndex++;
                        }
                        else
                        {
                            for (int i = targetIndex; i >= byteStart; i--)
                                target[i + 2] = target[i];

                            target[byteStart] = (uint)(249 + ((targetIndex - byteStart) / 250));
                            target[byteStart + 1] = (uint)((targetIndex - byteStart) % 250);
                            targetIndex += 2;
                        }
                    }
                    else
                    {
                        target[targetIndex] = barcodeData[sourceIndex];
                        targetIndex++;
                        sourceIndex++;
                    }
                }

                if (targetIndex > 1480)
                    return 0;   // Data is too large for symbol.

            } while (sourceIndex < inputLength);

            // Empty buffers.
            if (c40Index == 2)
            {
                int iv;

                c40Buffer[2] = 1;
                iv = (1600 * c40Buffer[0]) + (40 * c40Buffer[1]) + (c40Buffer[2]) + 1;
                target[targetIndex] = (uint)(iv / 256);
                targetIndex++;
                target[targetIndex] = (uint)(iv % 256);
                targetIndex++;
                target[targetIndex] = 255;
                targetIndex++; // Unlatch.
            }

            if (c40Index == 1)
            {
                int iv;

                c40Buffer[1] = 1;
                c40Buffer[2] = 31; // Pad.
                iv = (1600 * c40Buffer[0]) + (40 * c40Buffer[1]) + (c40Buffer[2]) + 1;
                target[targetIndex] = (uint)(iv / 256);
                targetIndex++;
                target[targetIndex] = (uint)(iv % 256);
                targetIndex++;
                target[targetIndex] = 255;
                targetIndex++; // Unlatch.
            }

            if (textIndex == 2)
            {
                int iv;

                textBuffer[2] = 1;
                iv = (1600 * textBuffer[0]) + (40 * textBuffer[1]) + (textBuffer[2]) + 1;
                target[targetIndex] = (uint)(iv / 256);
                targetIndex++;
                target[targetIndex] = (uint)(iv % 256);
                targetIndex++;
                target[targetIndex] = 255;
                targetIndex++; // Unlatch.
            }

            if (textIndex == 1)
            {
                int iv;

                textBuffer[1] = 1;
                textBuffer[2] = 31; // Pad.
                iv = (1600 * textBuffer[0]) + (40 * textBuffer[1]) + (textBuffer[2]) + 1;
                target[targetIndex] = (uint)(iv / 256);
                targetIndex++;
                target[targetIndex] = (uint)(iv % 256);
                targetIndex++;
                target[targetIndex] = 255;
                targetIndex++; // Unlatch.
            }

            if (currentMode == DECIMAL)
            {
                int bitsLeftInByte, targetCount;
                // Finish Decimal mode and go back to ASCII.
                decimalBinary.AppendBits(63, 6); // Unlatch. "111111"

                targetCount = 3;
                if (decimalBinary.SizeInBits <= 16)
                    targetCount = 2;

                if (decimalBinary.SizeInBits <= 8)
                    targetCount = 1;

                bitsLeftInByte = (8 * targetCount) - decimalBinary.SizeInBits;
                if (bitsLeftInByte == 8)
                    bitsLeftInByte = 0;

                if (bitsLeftInByte == 2)
                    decimalBinary.AppendBits(1, 2); // "01"

                if ((bitsLeftInByte == 4) || (bitsLeftInByte == 6))
                    decimalBinary.AppendBits(15, 4);   // "1111"

                if (bitsLeftInByte == 6)
                    decimalBinary.AppendBits(1, 2); // "01"

                // Binary buffer is full - transfer to target.
                binaryBytes = decimalBinary.ToByteArray();
                if (targetCount >= 1)
                {
                    target[targetIndex] = (uint)binaryBytes[0];
                    targetIndex++;
                }

                if (targetCount >= 2)
                {
                    target[targetIndex] = (uint)binaryBytes[1];
                    targetIndex++;
                }

                if (targetCount == 3)
                {
                    target[targetIndex] = (uint)binaryBytes[2];
                    targetIndex++;
                }
            }

            if (currentMode == BYTE)
            {
                // Insert byte field inputLength
                if ((targetIndex - byteStart) <= 249)
                {
                    for (int i = targetIndex; i >= byteStart; i--)
                        target[i + 1] = target[i];

                    target[byteStart] = (uint)(targetIndex - byteStart);
                    targetIndex++;
                }

                else
                {
                    for (int i = targetIndex; i >= byteStart; i--)
                        target[i + 2] = target[i];

                    target[byteStart] = (uint)(249 + ((targetIndex - byteStart) / 250));
                    target[byteStart + 1] = (uint)((targetIndex - byteStart) % 250);
                    targetIndex += 2;
                }
            }

            // Re-check input length of data.
            if (targetIndex > 1480)
                return 0;   // Data is too large for symbol.

            return targetIndex;
        }

        private bool dq4bi(int position, int inputLength)
        {
            int i;

            for (i = position; IsEdifact(barcodeData[position + i]) && ((position + i) < inputLength); i++) ;

            if ((position + i) == inputLength)
                return false;   // Reached end of input.

            if (barcodeData[position + i - 1] == 13)
                return true;

            if (barcodeData[position + i - 1] == '*')
                return true;

            if (barcodeData[position + i - 1] == '>')
                return true;

            return false;
        }

        private int LookAheadTest(int position, int inputLength, int current_mode)
        {
            float asciiCount, c40Count, textCount, edifactCount, byteCount;
            char reducedChar;
            bool done;
            int bestScheme, bestCount, sourceIndex;

            // Step J.
            if (current_mode == ASCII)
            {
                asciiCount = 0.0f;
                c40Count = 1.0f;
                textCount = 1.0f;
                edifactCount = 1.0f;
                byteCount = 2.0f;
            }

            else
            {
                asciiCount = 1.0f;
                c40Count = 2.0f;
                textCount = 2.0f;
                edifactCount = 2.0f;
                byteCount = 3.0f;
            }

            switch (current_mode)
            {
                case C40: c40Count = 0.0f;
                    break;

                case TEXT: textCount = 0.0f;
                    break;

                case BYTE: byteCount = 0.0f;
                    break;

                case EDIFACT: edifactCount = 0.0f;
                    break;
            }

            for (sourceIndex = position; (sourceIndex < inputLength) && (sourceIndex <= (position + 8)); sourceIndex++)
            {
                if (barcodeData[sourceIndex] <= 127)
                    reducedChar = barcodeData[sourceIndex];

                else
                    reducedChar = (char)(barcodeData[sourceIndex] - 127);

                /* Step L */
                if ((barcodeData[sourceIndex] >= '0') && (barcodeData[sourceIndex] <= '9'))
                    asciiCount += 0.5f;

                else
                {
                    asciiCount = (float)Math.Ceiling(asciiCount);
                    if (barcodeData[sourceIndex] > 127)
                        asciiCount += 2.0f;

                    else
                        asciiCount += 1.0f;
                }

                /* Step M */
                done = false;
                if (reducedChar == ' ')
                {
                    c40Count += (2.0f / 3.0f);
                    done = true;
                }
                if ((reducedChar >= '0') && (reducedChar <= '9'))
                {
                    c40Count += (2.0f / 3.0f);
                    done = true;
                }

                if ((reducedChar >= 'A') && (reducedChar <= 'Z'))
                {
                    c40Count += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] > 127)
                {
                    c40Count += (4.0f / 3.0f);
                }

                if (done == false)
                {
                    c40Count += (4.0f / 3.0f);
                }

                // Step N.
                done = false;
                if (reducedChar == ' ')
                {
                    textCount += (2.0f / 3.0f);
                    done = true;
                }

                if ((reducedChar >= '0') && (reducedChar <= '9'))
                {
                    textCount += (2.0f / 3.0f);
                    done = true;
                }

                if ((reducedChar >= 'a') && (reducedChar <= 'z'))
                {
                    textCount += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] > 127)
                {
                    textCount += (4.0f / 3.0f);
                }

                if (done == false)
                {
                    textCount += (4.0f / 3.0f);
                }

                // Step O.
                done = false;
                if (barcodeData[sourceIndex] == 13)
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] == '*')
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] == '>')
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] == ' ')
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }
                if ((barcodeData[sourceIndex] >= '0') && (barcodeData[sourceIndex] <= '9'))
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }

                if ((barcodeData[sourceIndex] >= 'A') && (barcodeData[sourceIndex] <= 'Z'))
                {
                    edifactCount += (2.0f / 3.0f);
                    done = true;
                }

                if (barcodeData[sourceIndex] > 127)
                    edifactCount += (13.0f / 3.0f);

                else
                {
                    if (done == false)
                        edifactCount += (10.0f / 3.0f);
                }

                // Step P.
                if (isGS1 && (barcodeData[sourceIndex] == '['))
                    byteCount += 3.0f;

                else
                    byteCount += 1.0f;
            }

            asciiCount = (float)Math.Ceiling(asciiCount);
            c40Count = (float)Math.Ceiling(c40Count);
            textCount = (float)Math.Ceiling(textCount);
            edifactCount = (float)Math.Ceiling(edifactCount);
            byteCount = (float)Math.Ceiling(byteCount);
            bestScheme = ASCII;

            if (sourceIndex == inputLength)
            {
                // Step K.
                bestCount = (int)edifactCount;

                if (textCount <= bestCount)
                {
                    bestCount = (int)textCount;
                    bestScheme = TEXT;
                }

                if (c40Count <= bestCount)
                {
                    bestCount = (int)c40Count;
                    bestScheme = C40;
                }

                if (asciiCount <= bestCount)
                {
                    bestCount = (int)asciiCount;
                    bestScheme = ASCII;
                }

                if (byteCount <= bestCount)
                {
                    bestCount = (int)byteCount;
                    bestScheme = BYTE;
                }
            }
            else
            {
                // Step Q .
                if (((edifactCount + 1.0 <= asciiCount) && (edifactCount + 1.0 <= c40Count)) && ((edifactCount + 1.0 <= byteCount) && (edifactCount + 1.0 <= textCount)))
                    bestScheme = EDIFACT;

                if ((c40Count + 1.0 <= asciiCount) && (c40Count + 1.0 <= textCount))
                {
                    if (c40Count < edifactCount)
                        bestScheme = C40;

                    else
                    {
                        done = false;
                        if (c40Count == edifactCount)
                        {
                            if (dq4bi(position, inputLength))
                                bestScheme = EDIFACT;

                            else
                                bestScheme = C40;
                        }
                    }
                }

                if (((textCount + 1.0 <= asciiCount) && (textCount + 1.0 <= c40Count)) && ((textCount + 1.0 <= byteCount) && (textCount + 1.0 <= edifactCount)))
                    bestScheme = TEXT;

                if (((asciiCount + 1.0 <= byteCount) && (asciiCount + 1.0 <= c40Count)) && ((asciiCount + 1.0 <= textCount) && (asciiCount + 1.0 <= edifactCount)))
                    bestScheme = ASCII;

                if (((byteCount + 1.0 <= asciiCount) && (byteCount + 1.0 <= c40Count)) && ((byteCount + 1.0 <= textCount) && (byteCount + 1.0 <= edifactCount)))
                    bestScheme = BYTE;
            }

            return bestScheme;
        }

        private static void BlockCopy(byte[] symbolGrid,  int symbolWidth, char[,] grid, int start_row, int start_col, int height, int width, int row_offset, int col_offset)
        {
            for (int i = start_row; i < (start_row + height); i++)
            {
                for (int j = start_col; j < (start_col + width); j++)
                {
                    if (grid[i, j] == '1')
                        SetGridModule(symbolGrid, symbolWidth, i + row_offset, j + col_offset, 1);
                }
            }
        }

        private static void Spigot(byte[] symbolGrid, int symbolWidth, int rowNumber)
        {
            for (int i = symbolWidth - 1; i > 0; i--)
            {
                if (GridModuleSet(symbolGrid, symbolWidth, rowNumber, i - 1))
                    SetGridModule(symbolGrid, symbolWidth, rowNumber, i, 1);
            }
        }

        private static void CentralFinder(byte[] symbolGrid, int symbolWidth, int startRow, int rowCount, int fullRows)
        {
            for (int i = 0; i < rowCount; i++)
            {
                if (i < fullRows)
                    Horizontal(symbolGrid, symbolWidth, startRow + (i * 2), true);

                else
                {
                    Horizontal(symbolGrid, symbolWidth, startRow + (i * 2), false);
                    if (i != rowCount - 1)
                    {
                        SetGridModule(symbolGrid, symbolWidth, startRow + (i * 2) + 1, 1, 1);
                        SetGridModule(symbolGrid, symbolWidth, startRow + (i * 2) + 1, symbolWidth - 2, 1);
                    }
                }
            }
        }

        private static void Horizontal(byte[] symbolGrid, int symbolWidth, int rowNumber, bool full)
        {
            if (full)
            {
                for (int i = 0; i < symbolWidth; i++)
                    SetGridModule(symbolGrid, symbolWidth, rowNumber, i, 1);
            }

            else
            {
                for (int i = 1; i < symbolWidth - 1; i++)
                    SetGridModule(symbolGrid, symbolWidth, rowNumber, i, 1);
            }
        }

        private static void Verticle(byte[] symbolGrid, int symbolRows, int symbolWidth, int column, int height, bool isTop)
        {

            if (isTop)
            {
                for (int i = 0; i < height; i++)
                    SetGridModule(symbolGrid, symbolWidth, i, column, 1);
            }

            else
            {
                for (int i = 0; i < height; i++)
                    SetGridModule(symbolGrid, symbolWidth, symbolRows - i - 1, column, 1);
            }
        }


        /// <summary>
        /// Test for special EDIFACT characters
        /// </summary>
        /// <param name="input">charcter to test</param>
        /// <returns>true if special character, otherwise false</returns>
        private static bool IsEdifact(char input)
        {
            bool result = false;

            if (input == (char)13)
                result = true;

            if (input == '*')
                result = true;

            if (input == '>')
                result = true;

            if (input == ' ')
                result = true;

            if (Char.IsDigit(input))
                result = true;

            if (Char.IsUpper(input))
                result = true;

            return result;
        }

        private static void SetGridModule(byte[] symbolGrid, int symbolWidth, int row, int column, byte value)
        {
            symbolGrid[(row * symbolWidth) + column] = value;
        }

        /// <summary>
        /// Test if the specified grid module is set.
        /// </summary>
        /// <param name="symbolGrid"></param>
        /// <param name="symbolWidth"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private static bool GridModuleSet(byte[] symbolGrid, int symbolWidth, int row, int column)
        {
            bool result = false;

            if (symbolGrid[(row * symbolWidth) + column] == 1)
                result = true;

            return result;
        }
    }
}
