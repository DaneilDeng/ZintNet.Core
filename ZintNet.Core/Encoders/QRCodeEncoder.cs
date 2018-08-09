/* QRCodeEncoder.cs Handles QR and Micro QR 2D symbols */

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
using System.Collections.ObjectModel;
using System.Text;


namespace ZintNet.Core.Encoders
{
    internal partial class QRCodeEncoder : SymbolEncoder
    {
        #region Tables

        // From ISO/IEC 18004:2006 Table 7.
        private static int[] QRDataCodeWordsLow = {
	        19, 34, 55, 80, 108, 136, 156, 194, 232, 274, 324, 370, 428, 461, 523, 589, 647,
	        721, 795, 861, 932, 1006, 1094, 1174, 1276, 1370, 1468, 1531, 1631,
	        1735, 1843, 1955, 2071, 2191, 2306, 2434, 2566, 2702, 2812, 2956};

        private static int[] QRDataCodeWordsMedium = {
	        16, 28, 44, 64, 86, 108, 124, 154, 182, 216, 254, 290, 334, 365, 415, 453, 507,
	        563, 627, 669, 714, 782, 860, 914, 1000, 1062, 1128, 1193, 1267,
	        1373, 1455, 1541, 1631, 1725, 1812, 1914, 1992, 2102, 2216, 2334};

        private static int[] QRDataCodeWordsQuartile = {
	        13, 22, 34, 48, 62, 76, 88, 110, 132, 154, 180, 206, 244, 261, 295, 325, 367,
	        397, 445, 485, 512, 568, 614, 664, 718, 754, 808, 871, 911,
	        985, 1033, 1115, 1171, 1231, 1286, 1354, 1426, 1502, 1582, 1666};

        private static int[] QRDataCodeWordsHigh = {
	        9, 16, 26, 36, 46, 60, 66, 86, 100, 122, 140, 158, 180, 197, 223, 253, 283,
	        313, 341, 385, 406, 442, 464, 514, 538, 596, 628, 661, 701,
	        745, 793, 845, 901, 961, 986, 1054, 1096, 1142, 1222, 1276};

        private static int[] QRTotalCodeWords = {
            // Total code words Data + Ecc.
	        26, 44, 70, 100, 134, 172, 196, 242, 292, 346, 404, 466, 532, 581, 655, 733, 815,
	        901, 991, 1085, 1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051,
	        2185, 2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706};

        private static int[] QRBlocksLow = {
	        1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12,
	        12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25};

        private static int[] QRBlocksMedium = {
	        1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20,
	        21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49};

        private static int[] QRBlocksQuartile = {
	        1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25,
	        27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68};

        private static int[] QRBlocksHigh = {
	        1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30,
	        32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81};

        private static int[] QRSizes = {
	        21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69, 73, 77, 81, 85, 89, 93, 97,
	        101, 105, 109, 113, 117, 121, 125, 129, 133, 137, 141, 145, 149, 153, 157, 161, 165, 169, 173, 177};

        private static int[] QRAlignmentLoopSizes = {
	        0, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4,
            5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7};

        private static int[] QRTableE1 = {
	        6, 18, 0, 0, 0, 0, 0, 6, 22, 0, 0, 0, 0, 0, 6, 26, 0, 0, 0, 0, 0, 6, 30, 0, 0, 0, 0, 0,
	        6, 34, 0, 0, 0, 0, 0, 6, 22, 38, 0, 0, 0, 0, 6, 24, 42, 0, 0, 0, 0, 6, 26, 46, 0, 0, 0, 0,
	        6, 28, 50, 0, 0, 0, 0, 6, 30, 54, 0, 0, 0, 0, 6, 32, 58, 0, 0, 0, 0, 6, 34, 62, 0, 0, 0, 0,
	        6, 26, 46, 66, 0, 0, 0, 6, 26, 48, 70, 0, 0, 0, 6, 26, 50, 74, 0, 0, 0, 6, 30, 54, 78, 0, 0, 0,
	        6, 30, 56, 82, 0, 0, 0, 6, 30, 58, 86, 0, 0, 0, 6, 34, 62, 90, 0, 0, 0, 6, 28, 50, 72, 94, 0, 0,
	        6, 26, 50, 74, 98, 0, 0, 6, 30, 54, 78, 102, 0, 0, 6, 28, 54, 80, 106, 0, 0, 6, 32, 58, 84, 110, 0, 0,
	        6, 30, 58, 86, 114, 0, 0, 6, 34, 62, 90, 118, 0, 0, 6, 26, 50, 74, 98, 122, 0, 6, 30, 54, 78, 102, 126, 0,
	        6, 26, 52, 78, 104, 130, 0, 6, 30, 56, 82, 108, 134, 0, 6, 34, 60, 86, 112, 138, 0, 6, 30, 58, 86, 114, 142, 0,
	        6, 34, 62, 90, 118, 146, 0, 6, 30, 54, 78, 102, 126, 150, 6, 24, 50, 76, 102, 128, 154, 6, 28, 54, 80, 106, 132, 158,
	        6, 32, 58, 84, 110, 136, 162, 6, 26, 54, 82, 110, 138, 166, 6, 30, 58, 86, 114, 142, 170};

        private static UInt32[] QRFormatSeqence = {
	        // Format information bit sequences.
	        0x5412, 0x5125, 0x5e7c, 0x5b4b, 0x45f9, 0x40ce, 0x4f97, 0x4aa0, 0x77c4, 0x72f3, 0x7daa, 0x789d,
	        0x662f, 0x6318, 0x6c41, 0x6976, 0x1689, 0x13be, 0x1ce7, 0x19d0, 0x0762, 0x0255, 0x0d0c, 0x083b,
	        0x355f, 0x3068, 0x3f31, 0x3a06, 0x24b4, 0x2183, 0x2eda, 0x2bed};

        private static int[] QRVersionSeqence = {
	        // Version information bit sequences.
	        0x07c94, 0x085bc, 0x09a99, 0x0a4d3, 0x0bbf6, 0x0c762, 0x0d847, 0x0e60d, 0x0f928, 0x10b78,
	        0x1145d, 0x12a17, 0x13532, 0x149a6, 0x15683, 0x168c9, 0x177ec, 0x18ec4, 0x191e1, 0x1afab,
	        0x1b08e, 0x1cc1a, 0x1d33f, 0x1ed75, 0x1f250, 0x209d5, 0x216f0, 0x228ba, 0x2379f, 0x24b0b,
	        0x2542e, 0x26a64, 0x27541, 0x28c69};

        private static int[] MicroQRFormatSequence = {
	        // Micro QR Code format information.
	        0x4445, 0x4172, 0x4e2b, 0x4b1c, 0x55ae, 0x5099, 0x5fc0, 0x5af7, 0x6793, 0x62a4, 0x6dfd, 0x68ca, 0x7678, 0x734f,
	        0x7c16, 0x7921, 0x06de, 0x03e9, 0x0cb0, 0x0987, 0x1735, 0x1202, 0x1d5b, 0x186c, 0x2508, 0x203f, 0x2f66, 0x2a51, 0x34e3,
	        0x31d4, 0x3e8d, 0x3bba
        };

        private static int[] MicroQRSizes = { 11, 13, 15, 17 };

        #endregion

        private QRCodeErrorLevel optionErrorCorrection;
        private int optionSymbolVersion;

        public QRCodeEncoder(Symbology symbology, string barcodeMessage, int optionSymbolVersion,
            QRCodeErrorLevel optionErrorCorrection, int eci, EncodingMode mode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionErrorCorrection = optionErrorCorrection;
            this.optionSymbolVersion = optionSymbolVersion;
            this.eci = eci;
            this.encodingMode = mode;
            this.Symbol = new Collection<SymbolData>();
        }

        public override Collection<SymbolData> EncodeData()
        {
            switch (symbolId)
            {
                case Symbology.QRCode:
                    switch (encodingMode)
                    {
                        case EncodingMode.Standard:
                            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                            break;

                        case EncodingMode.GS1:
                            isGS1 = true;
                            barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                            break;

                        case EncodingMode.HIBC:
                            barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                            break;
                    }

                    QRCode();
                    break;

                case Symbology.MicroQRCode:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    MicroQRCode();
                    break;

                case Symbology.UPNQR:
                    barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                    UPNQR();
                    break;
            }

            return Symbol;
        }

        private void QRCode()
        {
            int autoSize;
            int version;
            int symbolSize;
            int estimatedBinaryLength;
            BitVector bitStream;
            bool canShrink;
            QRCodeErrorLevel eccLevel = QRCodeErrorLevel.Low;
            int maxCodewords = 2956;
            int inputLength = barcodeData.Length;
            char[] jisData = new char[inputLength];
            char[] mode = new char[inputLength];

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] <= 0xff)
                    jisData[i] = barcodeData[i];

                else
                    jisData[i] = GetShiftJISCharacter(barcodeData[i]);
            }

            DefineModes(mode, jisData, inputLength);
            estimatedBinaryLength = EstimateBinaryLength(mode, inputLength);

            switch (optionErrorCorrection)
            {
                case QRCodeErrorLevel.Low:
                    eccLevel = QRCodeErrorLevel.Low;
                    maxCodewords = 2956;
                    break;

                case QRCodeErrorLevel.Medium:
                    eccLevel = QRCodeErrorLevel.Medium;
                    maxCodewords = 2334;
                    break;

                case QRCodeErrorLevel.Quartile:
                    eccLevel = QRCodeErrorLevel.Quartile;
                    maxCodewords = 1666;
                    break;

                case QRCodeErrorLevel.High:
                    eccLevel = QRCodeErrorLevel.High;
                    maxCodewords = 1276;
                    break;
            }

            if (estimatedBinaryLength > (8 * maxCodewords))
                throw new InvalidDataLengthException("QR Code: Input data too long for selected error correction level.");

            autoSize = 40;
            for (int i = 39; i >= 0; i--)
            {
                switch (eccLevel)
                {
                    case QRCodeErrorLevel.Low:
                        if (8 * QRDataCodeWordsLow[i] >= estimatedBinaryLength)
                            autoSize = i + 1;
                        break;

                    case QRCodeErrorLevel.Medium:
                        if (8 * QRDataCodeWordsMedium[i] >= estimatedBinaryLength)
                            autoSize = i + 1;
                        break;

                    case QRCodeErrorLevel.Quartile:
                        if (8 * QRDataCodeWordsQuartile[i] >= estimatedBinaryLength)
                            autoSize = i + 1;
                        break;

                    case QRCodeErrorLevel.High:
                        if (8 * QRDataCodeWordsHigh[i] >= estimatedBinaryLength)
                            autoSize = i + 1;
                        break;
                }
            }

            // Now see if the optimised binary will fit in a smaller symbol.
            canShrink = true;
            do
            {
                if (autoSize == 1)
                    canShrink = false;

                else
                {
                    if (Tribus(autoSize - 1, 1, 2, 3) != Tribus(autoSize, 1, 2, 3))
                        estimatedBinaryLength = GetBinaryLength(autoSize - 1, mode, jisData, inputLength);

                    switch (eccLevel)
                    {
                        case QRCodeErrorLevel.Low:
                            if (8 * QRDataCodeWordsLow[autoSize - 2] < estimatedBinaryLength)
                                canShrink = false;
                            break;

                        case QRCodeErrorLevel.Medium:
                            if (8 * QRDataCodeWordsMedium[autoSize - 2] < estimatedBinaryLength)
                                canShrink = false;
                            break;

                        case QRCodeErrorLevel.Quartile:
                            if (8 * QRDataCodeWordsQuartile[autoSize - 2] < estimatedBinaryLength)
                                canShrink = false;
                            break;

                        case QRCodeErrorLevel.High:
                            if (8 * QRDataCodeWordsHigh[autoSize - 2] < estimatedBinaryLength)
                                canShrink = false;
                            break;
                    }

                    if (canShrink)
                        autoSize--;

                    else
                    {
                        // Data did not fit in the smaller symbol, revert to original size
                        if (Tribus(autoSize - 1, 1, 2, 3) != Tribus(autoSize, 1, 2, 3))
                            estimatedBinaryLength = GetBinaryLength(autoSize, mode, jisData, inputLength);
                    }
                }
            } while (canShrink);

            version = autoSize;

            if ((optionSymbolVersion >= 1) && (optionSymbolVersion <= 40))
            {
                /* If the user has selected a larger symbol than the smallest available,
                 then use the size the user has selected, and re-optimise for this
                 symbol size. */
                if (optionSymbolVersion > version)
                {
                    version = optionSymbolVersion;
                    estimatedBinaryLength = GetBinaryLength(optionSymbolVersion, mode, jisData, inputLength);
                }
            }

            // Ensure maximum error correction capacity.
            if (estimatedBinaryLength <= QRDataCodeWordsMedium[version - 1])
                eccLevel = QRCodeErrorLevel.Medium;

            if (estimatedBinaryLength <= QRDataCodeWordsQuartile[version - 1])
                eccLevel = QRCodeErrorLevel.Quartile;

            if (estimatedBinaryLength <= QRDataCodeWordsHigh[version - 1])
                eccLevel = QRCodeErrorLevel.High;

            int dataCodewords = QRDataCodeWordsLow[version - 1];
            int blocks = QRBlocksLow[version - 1];
            switch (eccLevel)
            {
                case QRCodeErrorLevel.Medium:
                    dataCodewords = QRDataCodeWordsMedium[version - 1];
                    blocks = QRBlocksMedium[version - 1];
                    break;

                case QRCodeErrorLevel.Quartile:
                    dataCodewords = QRDataCodeWordsQuartile[version - 1];
                    blocks = QRBlocksQuartile[version - 1];
                    break;

                case QRCodeErrorLevel.High:
                    dataCodewords = QRDataCodeWordsHigh[version - 1];
                    blocks = QRBlocksHigh[version - 1];
                    break;
            }

            bitStream = new BitVector(QRTotalCodeWords[version - 1]);
            QRBinary(bitStream, mode, jisData, version, dataCodewords);
            AddErrorCorrection(bitStream, version, dataCodewords, blocks);

            symbolSize = QRSizes[version - 1];
            byte[] symbolGrid = new byte[symbolSize * symbolSize];

            SetupGrid(symbolGrid, symbolSize, version);
            PopulateGrid(symbolGrid, symbolSize, bitStream);

            if (version >= 7)
                AddVersionInformation(symbolGrid, symbolSize, version);

            int bitMask = ApplyBitmask(symbolGrid, symbolSize, eccLevel);
            AddFormatInformationGrid(symbolGrid, symbolSize, eccLevel, bitMask);
            BuildSymbol(symbolGrid, symbolSize);
        }

        private void DefineModes(char[] mode, char[] jisData, int inputLength)
        {
            // Values placed into mode[] are: K = Kanji, B = Binary, A = Alphanumeric, N = Numeric.
            int modeLength;

            for (int i = 0; i < inputLength; i++)
            {
                if (jisData[i] > 0xff)
                    mode[i] = 'K';

                else
                {
                    mode[i] = 'B';
                    if (CharacterSets.AlphaNumericSet.IndexOf(jisData[i]) != -1)
                        mode[i] = 'A';

                    if (isGS1 && (jisData[i] == '['))
                        mode[i] = 'A';

                    if (Char.IsDigit(jisData[i]))
                        mode[i] = 'N';
                }
            }

            // If less than 6 numeric digits together then use the mode preceding.
            // i.e. if the previous mode is Alphnumeric stay in this mode,
            // else switch to Binary.
            for (int i = 0; i < inputLength; i++)
            {
                char lastMode = 'B';

                if (i > 0)
                    lastMode = mode[i - 1];

                if (mode[i] == 'N')
                {
                    if (((i != 0) && (mode[i - 1] != 'N')) || (i == 0))
                    {
                        modeLength = 0;
                        while (((modeLength + i) < inputLength) && (mode[modeLength + i] == 'N'))
                            modeLength++;

                        if (lastMode == 'K')
                            lastMode = 'B';

                        if (modeLength < 6)
                        {
                            for (int j = 0; j < modeLength; j++)
                                mode[i + j] = lastMode;
                        }
                    }
                }
            }

            // If less than 4 alphanumeric characters together then use Byte mode.
            for (int i = 0; i < inputLength; i++)
            {
                if (mode[i] == 'A')
                {
                    if (((i != 0) && (mode[i - 1] != 'A')) || (i == 0))
                    {
                        modeLength = 0;
                        while (((modeLength + i) < inputLength) && (mode[modeLength + i] == 'A'))
                            modeLength++;

                        if (modeLength < 4)
                        {
                            for (int j = 0; j < modeLength; j++)
                                mode[i + j] = 'B';
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Estimates the number of data codewords for the given input data.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="inputLength">length of the input data</param>
        /// <returns></returns>
        private int EstimateBinaryLength(char[] mode, int inputLength)
        {
            // Make an estimate of how long the binary stream will be.
            int count = 0;
            char current = (char)0;
            int aCount = 0;
            int nCount = 0;

            if (isGS1)
                count += 4;

            if (eci != 3)
                count += 12;

            for (int i = 0; i < inputLength; i++)
            {
                if (mode[i] != current)
                {
                    switch (mode[i])
                    {
                        case 'K':
                            count += 16;
                            current = 'K';
                            break;

                        case 'B':
                            count += 20;
                            current = 'B';
                            break;

                        case 'A':
                            count += 17;
                            current = 'A';
                            aCount = 0;
                            break;

                        case 'N':
                            count += 18;
                            current = 'N';
                            nCount = 0;
                            break;
                    }
                }

                switch (mode[i])
                {
                    case 'K':
                        count += 13;
                        break;

                    case 'B':
                        count += 8;
                        break;

                    case 'A':
                        aCount++;
                        if ((aCount & 1) == 0)
                        {
                            count += 5;        // 11 in total
                            aCount = 0;
                        }

                        else
                            count += 6;
                        break;

                    case 'N':
                        nCount++;
                        if ((nCount % 3) == 0)
                        {
                            count += 3;        // 10 in total
                            nCount = 0;
                        }

                        else if ((nCount & 1) == 0)
                            count += 3;        // 7 in total

                        else
                            count += 4;
                        break;
                }
            }

            return count;
        }

        private void QRBinary(BitVector bitStream, char[] mode, char[] inputData, int version, int dataCodewords)
        {
            int position = 0;
            int scheme = 0;
            bool percent;
            int index;
            int shortBlockLength;
            char currentMode;

            int inputLength = inputData.Length;

            if (isGS1)
                bitStream.AppendBits(0x05, 4);   // FNC1 "0101"

            if (eci != 3)
            {
                bitStream.AppendBits(7, 4);     // Mode indicator
                bitStream.AppendBits(eci, 8);   // ECI
            }

            if (version <= 9)
                scheme = 0;

            else if ((version >= 10) && (version <= 26))
                scheme = 2;

            else if (version >= 27)
                scheme = 4;

            percent = false;
            do
            {
                currentMode = mode[position];
                shortBlockLength = 0;
                do
                {
                    shortBlockLength++;
                } while (((shortBlockLength + position) < inputLength) && (mode[position + shortBlockLength] == currentMode));

                switch (currentMode)
                {
                    case 'K':
                        // Kanji mode & count indicators.
                        bitStream.AppendBits(0x08, 4);
                        bitStream.AppendBits(shortBlockLength, 8 + scheme);    // 8, 10, 12

                        // Character representation.
                        for (int i = 0; i < shortBlockLength; i++)
                        {
                            int jisValue = (int)(inputData[position + i]);

                            if (jisValue >= 0x8140 && jisValue <= 0x9ffc)
                                jisValue -= 0x8140;

                            else if (jisValue >= 0xe040 && jisValue <= 0xebbf)
                                jisValue -= 0xc140;

                            int product = ((jisValue >> 8) * 0xc0) + (jisValue & 0xff);
                            bitStream.AppendBits(product, 13);
                        }

                        break;

                    case 'B':
                        // Byte mode & count indicators.
                        bitStream.AppendBits(0x04, 4);
                        bitStream.AppendBits(shortBlockLength, (scheme > 0) ? 16 : 8); // 8, 16

                        // Character representation.
                        for (int i = 0; i < shortBlockLength; i++)
                        {
                            int value = (int)(inputData[position + i]);

                            if (isGS1 && (value == '[')) // FNC1.
                                value = 0x1d;

                            bitStream.AppendBits(value, 0x08);
                        }
                        break;

                    case 'A':
                        // Alphanumeric mode & count indicators.
                        bitStream.AppendBits(0x02, 4);
                        bitStream.AppendBits(shortBlockLength, 9 + scheme);    // 9, 11, 13

                        // Character representation.
                        index = 0;
                        while (index < shortBlockLength)
                        {
                            int count = 0;
                            int first = 0, second = 0, product = 0;
                            if (!percent)
                            {
                                if (isGS1 && inputData[position + index] == '%')
                                {
                                    first = CharacterSets.AlphaNumericSet.IndexOf('%');
                                    second = CharacterSets.AlphaNumericSet.IndexOf('%');
                                    count = 2;
                                    product = (first * 45) + second;
                                    index++;
                                }

                                else
                                {
                                    if (isGS1 && inputData[position + index] == '[')  // FNC1.
                                        first = CharacterSets.AlphaNumericSet.IndexOf('%');

                                    else
                                        first = CharacterSets.AlphaNumericSet.IndexOf(inputData[position + index]);

                                    count = 1;
                                    index++;
                                    product = first;

                                    if (index < shortBlockLength && mode[position + index] == 'A')
                                    {
                                        if (isGS1 && (inputData[position + index]) == '%')
                                        {
                                            second = CharacterSets.AlphaNumericSet.IndexOf('%');
                                            count = 2;
                                            product = (first * 45) + second;
                                            percent = true;
                                        }

                                        else
                                        {
                                            if (isGS1 && (inputData[position + index]) == '[')
                                                second = CharacterSets.AlphaNumericSet.IndexOf('%');    // FNC1.

                                            else
                                                second = CharacterSets.AlphaNumericSet.IndexOf(inputData[position + index]);

                                            count = 2;
                                            index++;
                                            product = (first * 45) + second;
                                        }
                                    }
                                }
                            }

                            else
                            {
                                first = CharacterSets.AlphaNumericSet.IndexOf('%');
                                count = 1;
                                index++;
                                product = first;
                                percent = false;
                                if (index < shortBlockLength && mode[position + index] == 'A')
                                {
                                    if (isGS1 && (inputData[position + index]) == '%')
                                    {
                                        second = CharacterSets.AlphaNumericSet.IndexOf('%');
                                        count = 2;
                                        product = (first * 45) + second;
                                        percent = true;
                                    }

                                    else
                                    {
                                        if (isGS1 && (inputData[position + index]) == '[')
                                            second = CharacterSets.AlphaNumericSet.IndexOf('%');    // FNC1.

                                        else
                                            second = CharacterSets.AlphaNumericSet.IndexOf(inputData[position + index]);

                                        count = 2;
                                        index++;
                                        product = (first * 45) + second;
                                    }
                                }
                            }

                            bitStream.AppendBits(product, count == 2 ? 11 : 6);
                        }
                        break;

                    case 'N':
                        // Numeric mode and count indicators.
                        bitStream.AppendBits(0x01, 4);
                        bitStream.AppendBits(shortBlockLength, 10 + scheme);    // 10, 12, 14

                        // Character representation.
                        index = 0;
                        while (index < shortBlockLength)
                        {
                            int count = 0;
                            int first = 0, second = 0, third = 0, product = 0;

                            first = CharacterSets.NumberOnlySet.IndexOf(inputData[position + index]);
                            count = 1;
                            product = first;

                            if (index + 1 < shortBlockLength && mode[position + index + 1] == 'N')
                            {
                                second = CharacterSets.NumberOnlySet.IndexOf(inputData[position + index + 1]);
                                count = 2;
                                product = (product * 10) + second;

                                if (index + 2 < shortBlockLength && mode[position + index + 2] == 'N')
                                {
                                    third = CharacterSets.NumberOnlySet.IndexOf(inputData[position + index + 2]);
                                    count = 3;
                                    product = (product * 10) + third;
                                }
                            }

                            bitStream.AppendBits(product, (3 * count) + 1);
                            index += count;
                        }

                        break;
                }

                position += shortBlockLength;
            } while (position < inputLength);

            // Terminator.
            bitStream.AppendBits(0, 4);

            // Padding bits.
            int padBits = 8 - (bitStream.SizeInBits % 8);
            if (padBits == 8)
                padBits = 0;

            bitStream.AppendBits(0, padBits);

            // Codeword padding.
            int byteCount = bitStream.SizeInBytes;
            bool latch = false;
            for (int i = byteCount; i < dataCodewords; i++)
            {
                if (!latch)
                    bitStream.AppendBits(0xec, 8);

                else
                    bitStream.AppendBits(0x11, 8);

                latch = !latch;
            }
        }

        /// <summary>
        /// Adds error correction and interleaves the data.
        /// </summary>
        /// <param name="bitStream">binary encoded data</param>
        /// <param name="version">current QR version</param>
        /// <param name="dataCodewords">number of data codewords</param>
        /// <param name="blocks">number code blocks</param>
        private static void AddErrorCorrection(BitVector bitStream, int version, int dataCodewords, int blocks)
        {
            // Split data into blocks, add error correction and then interleave the blocks and error correction data.
            int eccCodewords = QRTotalCodeWords[version - 1] - dataCodewords;
            int shortDataBlockSize = dataCodewords / blocks;
            int shortBlocks = blocks - (dataCodewords % blocks);
            int eccBlockSize = eccCodewords / blocks;
            int dataBlockSize;

            byte[] dataBlocks = new byte[shortDataBlockSize + 1];
            byte[] eccBlocks = new byte[eccBlockSize];
            byte[] interleavedData = new byte[dataCodewords];
            byte[] interleavedEcc = new byte[eccCodewords];

            int position = 0;

            for (int i = 0; i < blocks; i++)
            {

                for (int j = 0; j < eccBlockSize; j++)
                    eccBlocks[j] = 0;

                if (i < shortBlocks)
                    dataBlockSize = shortDataBlockSize;

                else
                    dataBlockSize = shortDataBlockSize + 1;

                for (int j = 0; j < dataBlockSize; j++)
                    dataBlocks[j] = bitStream.ToByteArray()[position + j];

                ReedSolomon.RSInitialise(0x11d, eccBlockSize, 0);
                ReedSolomon.RSEncode(dataBlockSize, dataBlocks, eccBlocks);

                for (int j = 0; j < shortDataBlockSize; j++)
                    interleavedData[(j * blocks) + i] = dataBlocks[j];

                if (i >= shortBlocks)
                    interleavedData[(shortDataBlockSize * blocks) + (i - shortBlocks)] = dataBlocks[shortDataBlockSize];

                for (int j = 0; j < eccBlockSize; j++)
                    interleavedEcc[(j * blocks) + i] = eccBlocks[eccBlockSize - j - 1];

                position += dataBlockSize;
            }

            bitStream.Clear();
            for (int j = 0; j < dataCodewords; j++)
                bitStream.AppendBits(interleavedData[j], 8);

            for (int j = 0; j < eccCodewords; j++)
                bitStream.AppendBits(interleavedEcc[j], 8);
        }

        private static void SetupGrid(byte[] symbolGrid, int symbolSize, int version)
        {
            bool latch = true;

            // Add timing patterns.
            for (int i = 0; i < symbolSize; i++)
            {
                if (latch)
                {
                    symbolGrid[(6 * symbolSize) + i] = 0x21;
                    symbolGrid[(i * symbolSize) + 6] = 0x21;
                }

                else
                {
                    symbolGrid[(6 * symbolSize) + i] = 0x20;
                    symbolGrid[(i * symbolSize) + 6] = 0x20;
                }

                latch = !latch;
            }

            // Add finder patterns.
            PlaceFinderPatterns(symbolGrid, symbolSize, 0, 0);
            PlaceFinderPatterns(symbolGrid, symbolSize, 0, symbolSize - 7);
            PlaceFinderPatterns(symbolGrid, symbolSize, symbolSize - 7, 0);

            // Add separators.
            for (int i = 0; i < 7; i++)
            {
                symbolGrid[(7 * symbolSize) + i] = 0x10;
                symbolGrid[(i * symbolSize) + 7] = 0x10;
                symbolGrid[(7 * symbolSize) + (symbolSize - 1 - i)] = 0x10;
                symbolGrid[(i * symbolSize) + (symbolSize - 8)] = 0x10;
                symbolGrid[((symbolSize - 8) * symbolSize) + i] = 0x10;
                symbolGrid[((symbolSize - 1 - i) * symbolSize) + 7] = 0x10;
            }

            symbolGrid[(7 * symbolSize) + 7] = 0x10;
            symbolGrid[(7 * symbolSize) + (symbolSize - 8)] = 0x10;
            symbolGrid[((symbolSize - 8) * symbolSize) + 7] = 0x10;

            // Add alignment patterns
            // Version 1 does not have alignment patterns.
            if (version != 1)
            {
                int loopSize = QRAlignmentLoopSizes[version - 1];
                for (int x = 0; x < loopSize; x++)
                {
                    for (int y = 0; y < loopSize; y++)
                    {
                        int xcoord = QRTableE1[((version - 2) * 7) + x];
                        int ycoord = QRTableE1[((version - 2) * 7) + y];

                        if ((symbolGrid[(ycoord * symbolSize) + xcoord] & 0x10) == 0)
                            PlaceAlignmentPatterns(symbolGrid, symbolSize, xcoord, ycoord);
                    }
                }
            }

            // Reserve space for format information.
            for (int i = 0; i < 8; i++)
            {
                symbolGrid[(8 * symbolSize) + i] += 0x20;
                symbolGrid[(i * symbolSize) + 8] += 0x20;
                symbolGrid[(8 * symbolSize) + (symbolSize - 1 - i)] = 0x20;
                symbolGrid[((symbolSize - 1 - i) * symbolSize) + 8] = 0x20;
            }

            symbolGrid[(8 * symbolSize) + 8] += 20;
            symbolGrid[((symbolSize - 1 - 7) * symbolSize) + 8] = 0x21; // Dark Module from Figure 25.

            // Reserve space for version information.
            if (version >= 7)
            {
                for (int i = 0; i < 6; i++)
                {
                    symbolGrid[((symbolSize - 9) * symbolSize) + i] = 0x20;
                    symbolGrid[((symbolSize - 10) * symbolSize) + i] = 0x20;
                    symbolGrid[((symbolSize - 11) * symbolSize) + i] = 0x20;
                    symbolGrid[(i * symbolSize) + (symbolSize - 9)] = 0x20;
                    symbolGrid[(i * symbolSize) + (symbolSize - 10)] = 0x20;
                    symbolGrid[(i * symbolSize) + (symbolSize - 11)] = 0x20;
                }
            }
        }

        private static void PlaceAlignmentPatterns(byte[] symbolGrid, int symbolSize, int x, int y)
        {
            int[] alignment = { 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1 };

            x -= 2;
            y -= 2; // Input values represent centre of pattern.

            for (int xp = 0; xp < 5; xp++)
            {
                for (int yp = 0; yp < 5; yp++)
                {
                    if (alignment[xp + (5 * yp)] == 1)
                        symbolGrid[((yp + y) * symbolSize) + (xp + x)] = 0x11;

                    else
                        symbolGrid[((yp + y) * symbolSize) + (xp + x)] = 0x10;
                }
            }
        }

        private static void PlaceFinderPatterns(byte[] symbolGrid, int symbolSize, int x, int y)
        {
            int[] finder =
            {
                1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1,
		        1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1,
		        1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1,
		        1, 1, 1, 1, 1, 1, 1
            };

            for (int xp = 0; xp < 7; xp++)
            {
                for (int yp = 0; yp < 7; yp++)
                {
                    if (finder[xp + (7 * yp)] == 1)
                        symbolGrid[((yp + y) * symbolSize) + (xp + x)] = 0x11;

                    else
                        symbolGrid[((yp + y) * symbolSize) + (xp + x)] = 0x10;
                }
            }
        }

        private void PopulateGrid(byte[] symbolGrid, int symbolSize, BitVector bitStream)
        {
            int direction = 1;      // Up.
            int row = 0;            // Right hand side.
            int n = bitStream.SizeInBits;
            int y = symbolSize - 1;
            int i = 0;
            int x;

            do
            {
                x = (symbolSize - 2) - (row * 2);
                if (symbolId == Symbology.QRCode && x < 6)
                    x--;    // Skip over vertical timing pattern.

                if ((symbolGrid[(y * symbolSize) + (x + 1)] & 0xf0) == 0)
                {
                    symbolGrid[(y * symbolSize) + (x + 1)] = bitStream[i];
                    /*if (binaryStream.ValueAtIndex(i) == 1)
                        symbolGrid[(y * symbolSize) + (x + 1)] = 0x01;

                    else
                        symbolGrid[(y * symbolSize) + (x + 1)] = 0x00;*/

                    i++;
                }

                if (i < n)
                {
                    if ((symbolGrid[(y * symbolSize) + x] & 0xf0) == 0)
                    {
                        symbolGrid[(y * symbolSize) + x] = bitStream[i];
                        /*if (binaryStream.ValueAtIndex(i) == 1)
                            symbolGrid[(y * symbolSize) + x] = 0x01;

                        else
                            symbolGrid[(y * symbolSize) + x] = 0x00;*/

                        i++;
                    }
                }

                if (direction == 1)
                    y--;

                else
                    y++;

                if (y == -1)
                {
                    // Reached the top.
                    row++;
                    y = 0;
                    direction = 0;
                }

                if (y == symbolSize)
                {
                    // Reached the bottom.
                    row++;
                    y = symbolSize - 1;
                    direction = 1;
                }

            }
            while (i < n);
        }

        private static int Evaluate(byte[] evaluationData, int symbolSize, int pattern)
        {
            int block;
            int result = 0;
            byte state;
            int p = 0;
            int beforeCount, afterCount;

            byte[] local = new byte[symbolSize * symbolSize];

            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    byte value = (byte)(0x01 << pattern);
                    local[(y * symbolSize) + x] = (byte)((evaluationData[(y * symbolSize) + x] & value) != 0 ? 1 : 0);
                }
            }

            // Test 1: Adjacent modules in row/column are same colour.
            // Vertical.
            for (int x = 0; x < symbolSize; x++)
            {
                state = local[x];
                block = 0;
                for (int y = 0; y < symbolSize; y++)
                {
                    if (local[(y * symbolSize) + x] == state)
                        block++;

                    else
                    {
                        if (block > 5)
                            result += (3 + (block - 5));

                        block = 0;
                        state = local[(y * symbolSize) + x];
                    }
                }

                if (block > 5)
                    result += (3 + (block - 5));
            }

            // Horizontal.
            for (int y = 0; y < symbolSize; y++)
            {
                state = local[y * symbolSize];
                block = 0;
                for (int x = 0; x < symbolSize; x++)
                {
                    if (local[(y * symbolSize) + x] == state)
                        block++;

                    else
                    {
                        if (block > 5)
                            result += (3 + (block - 5));

                        block = 0;
                        state = local[(y * symbolSize) + x];
                    }
                }

                if (block > 5)
                    result += (3 + (block - 5));
            }

            // Test 2: Block of modules in same color.
            for (int x = 0; x < symbolSize - 1; x++)
            {
                for (int y = 0; y < (symbolSize - 1) - 1; y++)
                {
                    if ((local[(y * symbolSize) + x] == local[((y + 1) * symbolSize) + x]) &&
                        (local[(y * symbolSize) + x] == local[(y * symbolSize) + (x + 1)]) &&
                        (local[(y * symbolSize) + x] == local[((y + 1) * symbolSize) + (x + 1)]))
                        result += 3;
                }
            }

            // Test 3: 1:1:3:1:1 ratio pattern in row/column.
            // pattern 10111010000
            // Vertical
            int x3, y3;
            for (x3 = 0; x3 < symbolSize; x3++)
            {
                for (y3 = 0; y3 < (symbolSize - 7); y3++)
                {
                    p = 0;
                    for (int weight = 0; weight < 7; weight++)
                    {
                        if (local[((y3 + weight) * symbolSize) + x3] == '1')
                            p += (0x40 >> weight);

                    }
                }

                if (p == 0x5d)
                {
                    // Pattern found, check before and after.
                    beforeCount = 0;
                    for (int b = (y3 - 4); b < y3; b++)
                    {
                        if (b < 0)
                            beforeCount++;

                        else
                        {
                            if (local[(b * symbolSize) + x3] == '0')
                                beforeCount++;

                            else
                                beforeCount = 0;
                        }
                    }

                    afterCount = 0;
                    for (int a = (y3 + 7); a <= (y3 + 10); a++)
                    {
                        if (a >= symbolSize)
                            afterCount++;

                        else
                        {
                            if (local[(a * symbolSize) + x3] == '0')
                                afterCount++;

                            else
                                afterCount = 0;
                        }
                    }

                    if ((beforeCount == 4) || (afterCount == 4))    // Pattern is preceeded or followed by light area 4 modules wide.
                        result += 40;
                }
            }


            // Horizontal
            for (y3 = 0; y3 < symbolSize; y3++)
            {
                for (x3 = 0; x3 < (symbolSize - 7); x3++)
                {
                    p = 0;
                    for (int weight = 0; weight < 7; weight++)
                    {
                        if (local[(y3 * symbolSize) + x3 + weight] == '1')
                            p += (0x40 >> weight);
                    }

                    if (p == 0x5d)
                    {
                        // Pattern found, check before and after.
                        beforeCount = 0;
                        for (int b = (x3 - 4); b < x3; b++)
                        {
                            if (b < 0)
                                beforeCount++;

                            else
                            {
                                if (local[(y3 * symbolSize) + b] == '0')
                                    beforeCount++;

                                else
                                    beforeCount = 0;
                            }
                        }

                        afterCount = 0;
                        for (int a = (x3 + 7); a <= (x3 + 10); a++)
                        {
                            if (a >= symbolSize)
                                afterCount++;

                            else
                            {
                                if (local[(y3 * symbolSize) + a] == '0')
                                    afterCount++;

                                else
                                    afterCount = 0;
                            }
                        }

                        if ((beforeCount == 4) || (afterCount == 4))    // Pattern is preceeded or followed by light area 4 modules wide.
                            result += 40;
                    }
                }
            }

            // Test 4: Proportion of dark modules in entire symbol.
            int darkModules = 0;
            int k;

            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    if (local[(y * symbolSize) + x] == '1')
                        darkModules++;
                }
            }

            int percentage = 100 * (darkModules / (symbolSize * symbolSize));
            if (percentage <= 50)
                k = ((100 - percentage) - 50) / 5;

            else
                k = (percentage - 50) / 5;

            result += 10 * k;
            return result;
        }

        private static void AddFormatInformationEval(byte[] evaluationData, int symbolSize, QRCodeErrorLevel eccLevel, int pattern)
        {
            // Add format information to the evaluation data.
            int format = pattern;
            uint sequence;

            switch (eccLevel)
            {
                case QRCodeErrorLevel.Low:
                    format += 0x08;
                    break;

                case QRCodeErrorLevel.Quartile:
                    format += 0x18;
                    break;

                case QRCodeErrorLevel.High:
                    format += 0x10;
                    break;
            }

            sequence = QRFormatSeqence[format];

            for (int i = 0; i < 6; i++)
                evaluationData[(i * symbolSize) + 8] = (((sequence >> i) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;

            for (int i = 0; i < 8; i++)
                evaluationData[(8 * symbolSize) + (symbolSize - i - 1)] = (((sequence >> i) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;

            for (int i = 0; i < 6; i++)
                evaluationData[(8 * symbolSize) + (5 - i)] = (((sequence >> (i + 9)) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;

            for (int i = 0; i < 7; i++)
                evaluationData[(((symbolSize - 7) + i) * symbolSize) + 8] = (((sequence >> (i + 8)) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;

            evaluationData[(7 * symbolSize) + 8] = (((sequence >> 6) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;
            evaluationData[(8 * symbolSize) + 8] = (((sequence >> 7) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;
            evaluationData[(8 * symbolSize) + 7] = (((sequence >> 8) & 0x01) != 0) ? (byte)(0x01 >> pattern) : (byte)0x00;
        }

        private static void AddFormatInformationGrid(byte[] symbolGrid, int symbolSize, QRCodeErrorLevel eccLevel, int bitMask)
        {
            // Add format information to symbol grid.
            int formatInfo = bitMask;

            switch (eccLevel)
            {
                case QRCodeErrorLevel.Low:
                    formatInfo += 0x08;
                    break;

                case QRCodeErrorLevel.Quartile:
                    formatInfo += 0x18;
                    break;

                case QRCodeErrorLevel.High:
                    formatInfo += 0x10;
                    break;
            }

            uint seqence = QRFormatSeqence[formatInfo];

            for (int i = 0; i < 6; i++)
                symbolGrid[(i * symbolSize) + 8] += (byte)((seqence >> i) & 0x01);

            for (int i = 0; i < 8; i++)
                symbolGrid[(8 * symbolSize) + (symbolSize - i - 1)] += (byte)((seqence >> i) & 0x01);

            for (int i = 0; i < 6; i++)
                symbolGrid[(8 * symbolSize) + (5 - i)] += (byte)((seqence >> (i + 9)) & 0x01);

            for (int i = 0; i < 7; i++)
                symbolGrid[(((symbolSize - 7) + i) * symbolSize) + 8] += (byte)((seqence >> (i + 8)) & 0x01);

            symbolGrid[(7 * symbolSize) + 8] += (byte)((seqence >> 6) & 0x01);
            symbolGrid[(8 * symbolSize) + 8] += (byte)((seqence >> 7) & 0x01);
            symbolGrid[(8 * symbolSize) + 7] += (byte)((seqence >> 8) & 0x01);
        }

        private static int ApplyBitmask(byte[] symbolGrid, int symbolSize, QRCodeErrorLevel eccLevel)
        {
            byte p;
            int[] penalty = new int[8];

            byte[] mask = new byte[symbolSize * symbolSize];
            byte[] evaluation = new byte[symbolSize * symbolSize];

            // Perform data masking.
            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    mask[(y * symbolSize) + x] = 0x00;

                    if ((symbolGrid[(y * symbolSize) + x] & 0xf0) == 0) // Exclude areas not to be masked.
                    {
                        if (((y + x) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x01;

                        if ((y & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x02;

                        if ((x % 3) == 0)
                            mask[(y * symbolSize) + x] += 0x04;

                        if (((y + x) % 3) == 0)
                            mask[(y * symbolSize) + x] += 0x08;

                        if ((((y / 2) + (x / 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x10;

                        if ((((y * x) & 1) + ((y * x) % 3)) == 0)
                            mask[(y * symbolSize) + x] += 0x20;

                        if (((((y * x) & 1) + ((y * x) % 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x40;

                        if (((((y + x) & 1) + ((y * x) % 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x80;

                    }
                }
            }

            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    if ((symbolGrid[(y * symbolSize) + x] & 0x01) != 0)
                        p = 0xff;

                    else
                        p = 0x00;

                    evaluation[(y * symbolSize) + x] = (byte)(mask[(y * symbolSize) + x] ^ p);
                }
            }

            // Evaluate result.
            for (int pattern = 0; pattern < 8; pattern++)
            {
                AddFormatInformationEval(evaluation, symbolSize, eccLevel, pattern);
                penalty[pattern] = Evaluate(evaluation, symbolSize, pattern);
            }

            int bestPattern = 0;
            int bestValue = penalty[0];
            for (int pattern = 1; pattern < 8; pattern++)
            {
                if (penalty[pattern] < bestValue)
                {
                    bestPattern = pattern;
                    bestValue = penalty[pattern];
                }
            }

            // Apply mask.
            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    if ((mask[(y * symbolSize) + x] & (0x01 << bestPattern)) != 0)
                    {
                        {
                            if ((symbolGrid[(y * symbolSize) + x] & 0x01) != 0)
                                symbolGrid[(y * symbolSize) + x] = 0x00;

                            else
                                symbolGrid[(y * symbolSize) + x] = 0x01;
                        }
                    }
                }
            }

            return bestPattern;
        }

        private static void AddVersionInformation(byte[] symbolGrid, int symbolSize, int version)
        {
            // Add version information.
            int versionData = QRVersionSeqence[version - 7];
            for (int i = 0; i < 6; i++)
            {
                symbolGrid[((symbolSize - 11) * symbolSize) + i] += (byte)((versionData >> (i * 3)) & 0x41);
                symbolGrid[((symbolSize - 10) * symbolSize) + i] += (byte)((versionData >> ((i * 3) + 1)) & 0x41);
                symbolGrid[((symbolSize - 9) * symbolSize) + i] += (byte)((versionData >> ((i * 3) + 2)) & 0x41);
                symbolGrid[(i * symbolSize) + (symbolSize - 11)] += (byte)((versionData >> (i * 3)) & 0x01);
                symbolGrid[(i * symbolSize) + (symbolSize - 10)] += (byte)((versionData >> ((i * 3) + 1)) & 0x41);
                symbolGrid[(i * symbolSize) + (symbolSize - 9)] += (byte)((versionData >> ((i * 3) + 2)) & 0x41);
            }
        }

        /*private static bool IsShiftJIS(char data)
        {
            byte[] sjisBytes;
            char[] sjis = new char[1];
            char value;

            try
            {
                sjis[0] = data;
                sjisBytes = Encoding.GetEncoding("Shift_JIS",
                    new EncoderExceptionFallback(),
                    new DecoderExceptionFallback()).GetBytes(sjis);
            }

            catch (EncoderFallbackException e)
            {
                throw new InvalidDataException("QR Kanji Mode: Invalid byte sequence.", e);
            }

            value = (char)(sjisBytes[0] << 8 | sjisBytes[1] & 0xff);
            if ((value >= 0x8140 && value <= 0x9ffc) || (value >= 0xe040 && value <= 0xebbf))
                return true;

            return false;
        }*/

        private static char GetShiftJISCharacter(char data)
        {
            byte[] shiftJISBytes;
            char[] shiftJIS = new char[1];
            char shiftJISCharacter;

            try
            {
                shiftJIS[0] = data;
                shiftJISBytes = Encoding.GetEncoding("Shift_JIS",
                    new EncoderExceptionFallback(),
                    new DecoderExceptionFallback()).GetBytes(shiftJIS);
            }

            catch (EncoderFallbackException e)
            {
                throw new InvalidDataException("Shift_JIS: Invalid byte sequence.", e);
            }

            shiftJISCharacter = (char)(shiftJISBytes[0] << 8 | shiftJISBytes[1] & 0xff);
            return shiftJISCharacter;
        }

        /*private static char[] ConvertToKanji(string dataBlock)
        {
            byte[] sjisBytes;
            char[] kanjiCharacters;
            int position = 0;

            sjisBytes = Encoding.GetEncoding("Shift_JIS").GetBytes(dataBlock);
            kanjiCharacters = new char[sjisBytes.Length / 2];

            for (int i = 0; i < sjisBytes.Length; i += 2)
            {
                kanjiCharacters[position] = (char)((sjisBytes[i] << 8) | (sjisBytes[i + 1] & 0xff));
                position++;
            }

            return kanjiCharacters;
        }*/

        // Choose from three numbers based on version.
        private static int Tribus(int version, int a, int b, int c)
        {
            int returnValue;

            returnValue = c;

            if (version < 10)
                returnValue = a;

            if ((version >= 10) && (version <= 26))
                returnValue = b;

            return returnValue;
        }

        /// <summary>
        /// Implements a custom optimisation algorithm.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="inputMode"></param>
        /// <param name="inputLength"></param>
        private static void ApplyOptimisation(int version, char[] inputMode, int inputLength)
        {
            int blockCount = 0, block, j;
            char currentMode = ' '; // Null
            int[] blockLength;
            char[] blockMode;

            for (int i = 0; i < inputLength; i++)
            {
                if (inputMode[i] != currentMode)
                {
                    currentMode = inputMode[i];
                    blockCount++;
                }
            }

            blockLength = new int[blockCount];
            blockMode = new char[blockCount];

            j = -1;
            currentMode = ' ';
            for (int i = 0; i < inputLength; i++)
            {
                if (inputMode[i] != currentMode)
                {
                    j++;
                    blockLength[j] = 1;
                    blockMode[j] = inputMode[i];
                    currentMode = inputMode[i];
                }

                else
                    blockLength[j]++;
            }

            if (blockCount > 1)
            {
                // Search forward
                for (int i = 0; i <= (blockCount - 2); i++)
                {
                    if (blockMode[i] == 'B')
                    {
                        switch (blockMode[i + 1])
                        {
                            case 'K':
                                if (blockLength[i + 1] < Tribus(version, 4, 5, 6))
                                    blockMode[i + 1] = 'B';

                                break;

                            case 'A':
                                if (blockLength[i + 1] < Tribus(version, 7, 8, 9))
                                    blockMode[i + 1] = 'B';

                                break;

                            case 'N':
                                if (blockLength[i + 1] < Tribus(version, 3, 4, 5))
                                    blockMode[i + 1] = 'B';

                                break;
                        }
                    }

                    if ((blockMode[i] == 'A') && (blockMode[i + 1] == 'N'))
                    {
                        if (blockLength[i + 1] < Tribus(version, 6, 8, 10))
                            blockMode[i + 1] = 'A';
                    }
                }

                // Search backward
                for (int i = blockCount - 1; i > 0; i--)
                {
                    if (blockMode[i] == 'B')
                    {
                        switch (blockMode[i - 1])
                        {
                            case 'K':
                                if (blockLength[i - 1] < Tribus(version, 4, 5, 6))
                                    blockMode[i - 1] = 'B';

                                break;

                            case 'A':
                                if (blockLength[i - 1] < Tribus(version, 7, 8, 9))
                                    blockMode[i - 1] = 'B';

                                break;

                            case 'N':
                                if (blockLength[i - 1] < Tribus(version, 3, 4, 5))
                                    blockMode[i - 1] = 'B';

                                break;
                        }
                    }

                    if ((blockMode[i] == 'A') && (blockMode[i - 1] == 'N'))
                    {
                        if (blockLength[i - 1] < Tribus(version, 6, 8, 10))
                            blockMode[i - 1] = 'A';
                    }
                }
            }

            j = 0;
            for (block = 0; block < blockCount; block++)
            {
                currentMode = blockMode[block];
                for (int i = 0; i < blockLength[block]; i++)
                {
                    inputMode[j] = currentMode;
                    j++;
                }
            }

        }

        private static int BlockLength(int start, char[] inputMode, int inputLength)
        {
            /* Find the length of the block starting from 'start' */
            int i, count;
            char mode = inputMode[start];

            count = 0;
            i = start;

            do
            {
                count++;
            } while (((i + count) < inputLength) && (inputMode[i + count] == mode));

            return count;
        }

        /// <summary>
        /// Calculates the bit lengths of the data for selected version.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="inputMode"></param>
        /// <param name="inputData"></param>
        /// <param name="inputLength"></param>
        /// <returns></returns>
        private int GetBinaryLength(int version, char[] inputMode, char[] inputData, int inputLength)
        {
            int count = 0;
            char currentMode = ' ';

            ApplyOptimisation(version, inputMode, inputLength);
            if (isGS1 == true)
                count += 4;

            if (eci != 3)
                count += 12;

            for (int i = 0; i < inputLength; i++)
            {
                // Changing modes.
                if (inputMode[i] != currentMode)
                {
                    count += 4;
                    switch (inputMode[i])
                    {
                        case 'K':
                            count += Tribus(version, 8, 10, 12);
                            count += (BlockLength(i, inputMode, inputLength) * 13);
                            break;

                        case 'B':
                            count += Tribus(version, 8, 16, 16);
                            for (int j = i; j < (i + BlockLength(i, inputMode, inputLength)); j++)
                            {
                                if (inputData[j] > 0xff)
                                    count += 16;

                                else
                                    count += 8;
                            }
                            break;

                        case 'A':
                            count += Tribus(version, 9, 11, 13);
                            switch (BlockLength(i, inputMode, inputLength) % 2)
                            {
                                case 0:
                                    count += (BlockLength(i, inputMode, inputLength) / 2) * 11;
                                    break;

                                case 1:
                                    count += ((BlockLength(i, inputMode, inputLength) - 1) / 2) * 11;
                                    count += 6;
                                    break;
                            }
                            break;

                        case 'N':
                            count += Tribus(version, 10, 12, 14);
                            switch (BlockLength(i, inputMode, inputLength) % 3)
                            {
                                case 0:
                                    count += (BlockLength(i, inputMode, inputLength) / 3) * 10;
                                    break;

                                case 1:
                                    count += ((BlockLength(i, inputMode, inputLength) - 1) / 3) * 10;
                                    count += 4;
                                    break;

                                case 2:
                                    count += ((BlockLength(i, inputMode, inputLength) - 2) / 3) * 10;
                                    count += 7;
                                    break;
                            }
                            break;
                    }

                    currentMode = inputMode[i];
                }
            }

            return count;
        }

        private void BuildSymbol(byte[] symbolGrid, int symbolSize)
        {
            // Build the symbol.
            byte[] rowData;
            for (int i = 0; i < symbolSize; i++)
            {
                rowData = new byte[symbolSize];
                for (int j = 0; j < symbolSize; j++)
                {
                    if ((symbolGrid[(i * symbolSize) + j] & 0x01) > 0)
                        rowData[j] = 1;
                }

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(i, symbolData);
            }

        }

        /// <summary>
        /// MicroQR Encoder.
        /// </summary>
        private void MicroQRCode()
        {
            QRCodeErrorLevel eccLevel;
            int symbolSize;
            int version;
            BitVector bitStream = new BitVector();
            StringBuilder binaryString = new StringBuilder();
            int inputLength = barcodeData.Length;
            char[] mode = new char[inputLength];
            char[] jisData = new char[inputLength];
            bool kanjiUsed = false, alphanumUsed = false, byteUsed = false;
            int numericCount = 0, alphaCount = 0;
            int autoVersion;
            int[] versionValid = new int[4];
            int[] binaryCount = new int[4];

            if (inputLength > 35)
                throw new InvalidDataLengthException("MicoQR Code: Maximum input length is 35 characters.");

            for (int i = 0; i < 4; i++)
                versionValid[i] = 1;

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] <= 0xff)
                    jisData[i] = barcodeData[i];

                else
                    jisData[i] = GetShiftJISCharacter(barcodeData[i]);
            }

            DefineModes(mode, jisData, inputLength);
            for (int i = 0; i < inputLength; i++)
            {
                if (CharacterSets.NumberOnlySet.IndexOf(barcodeData[i]) != -1)
                    numericCount++;

                if (CharacterSets.AlphaNumericSet.IndexOf(barcodeData[i]) != -1)
                    alphaCount++;
            }

            if (alphaCount == inputLength)
            {
                // All data can be encoded in Alphanumeric mode.
                for (int i = 0; i < inputLength; i++)
                    mode[i] = 'A';
            }

            if (numericCount == inputLength)
            {
                // All data can be encoded in Numeric mode.
                for (int i = 0; i < inputLength; i++)
                    mode[i] = 'N';
            }

            MicroQRIntermediate(binaryString, jisData, mode, inputLength, ref kanjiUsed, ref alphanumUsed, ref byteUsed);
            char[] binaryStream = binaryString.ToString().ToCharArray();
            GetBitLength(binaryCount, binaryStream);

            // Eliminate possible versions depending on type of content.
            if (byteUsed)
            {
                versionValid[0] = 0;
                versionValid[1] = 0;
            }

            if (alphanumUsed)
                versionValid[0] = 0;

            if (kanjiUsed)
            {
                versionValid[0] = 0;
                versionValid[1] = 0;
            }

            // Eliminate possible versions depending on length of binary data.
            if (binaryCount[0] > 20)
                versionValid[0] = 0;

            if (binaryCount[1] > 40)
                versionValid[1] = 0;

            if (binaryCount[2] > 84)
                versionValid[2] = 0;

            if (binaryCount[3] > 128)
                throw new InvalidDataLengthException("MicroQR Code: Input data is too long.");

            // Eliminate possible versions depending on error correction level specified.
            eccLevel = QRCodeErrorLevel.Low;
            if ((int)optionErrorCorrection >= 1 && optionSymbolVersion <= 3)
                eccLevel = optionErrorCorrection;

            if (eccLevel == QRCodeErrorLevel.High)
                throw new ErrorCorrectionLevelException("MicroQR Code: High Error correction Level not supported.");

            if (eccLevel == QRCodeErrorLevel.Quartile)
            {
                versionValid[0] = 0;
                versionValid[1] = 0;
                versionValid[2] = 0;
                if (binaryCount[3] > 80)
                    throw new InvalidDataLengthException("MicroQR Code: Input data too long for Error Correction Level.");
            }

            if (eccLevel == QRCodeErrorLevel.Medium)
            {
                versionValid[0] = 0;
                if (binaryCount[1] > 32)
                    versionValid[1] = 0;

                if (binaryCount[2] > 68)
                    versionValid[2] = 0;

                if (binaryCount[3] > 112)
                    throw new InvalidDataLengthException("MicroQR Code: Input data too long for Error Correction Level.");
            }

            autoVersion = 3;
            if (versionValid[2] == 1)
                autoVersion = 2;

            if (versionValid[1] == 1)
                autoVersion = 1;

            if (versionValid[0] == 1)
                autoVersion = 0;

            version = autoVersion;
            // Get version from user.
            if (optionSymbolVersion >= 1 && optionSymbolVersion <= 4)
            {
                if ((optionSymbolVersion - 1) > autoVersion)
                    version = optionSymbolVersion - 1;
            }

            // If there is enough unused space then increase the error correction level.
            if (version == 3)
            {
                if (binaryCount[3] <= 112)
                    eccLevel = QRCodeErrorLevel.Medium;

                if (binaryCount[3] <= 80)
                    eccLevel = QRCodeErrorLevel.Quartile;
            }

            if (version == 2)
            {
                if (binaryCount[2] <= 68)
                    eccLevel = QRCodeErrorLevel.Medium;
            }

            if (version == 1)
            {
                if (binaryCount[1] <= 32)
                    eccLevel = QRCodeErrorLevel.Medium;
            }

            MicroQRExpandBinary(binaryStream, bitStream, version);
            switch (version)
            {
                case 0:
                    MicroQRM1(bitStream);
                    break;

                case 1:
                    MicroQRM2(bitStream, eccLevel);
                    break;

                case 2:
                    MicroQRM3(bitStream, eccLevel);
                    break;

                case 3:
                    MicroQRM4(bitStream, eccLevel);
                    break;
            }

            symbolSize = MicroQRSizes[version];
            byte[] symbolGrid = new byte[symbolSize * symbolSize];

            MicroSetupGrid(symbolGrid, symbolSize);
            MicroPopulateGrid(symbolGrid, symbolSize, bitStream);
            int bitmask = MicroApplyBitmask(symbolGrid, symbolSize);

            // Add format data.
            int format = 0;
            switch (version)
            {
                case 1:
                    switch (eccLevel)
                    {
                        case QRCodeErrorLevel.Low:
                            format = 1;
                            break;

                        case QRCodeErrorLevel.Medium:
                            format = 2;
                            break;
                    }
                    break;

                case 2:
                    switch (eccLevel)
                    {
                        case QRCodeErrorLevel.Low:
                            format = 3;
                            break;

                        case QRCodeErrorLevel.Medium:
                            format = 4;
                            break;
                    }
                    break;

                case 3: switch (eccLevel)
                    {
                        case QRCodeErrorLevel.Low:
                            format = 5;
                            break;

                        case QRCodeErrorLevel.Medium:
                            format = 6;
                            break;

                        case QRCodeErrorLevel.Quartile:
                            format = 7;
                            break;
                    }
                    break;
            }

            int formatSequence = MicroQRFormatSequence[(format << 2) + bitmask];
            int value = 0x4000;
            int count = 15;
            for (int x = 1; x < 16; x++)
            {
                if ((formatSequence & value) != 0)
                {
                    if (x < 9)
                        symbolGrid[(8 * symbolSize) + x] += 1;

                    else
                        symbolGrid[(count * symbolSize) + 8] += 1;
                }

                value /= 2;
                count--;
            }

            BuildSymbol(symbolGrid, symbolSize);
        }

        private static void MicroQRIntermediate(StringBuilder binaryString, char[] jisData, char[] mode, int length,
            ref bool kanjiUsed, ref bool alphanumUsed, ref bool byteUsed)
        {
            // Convert input data to an "intermediate stage" where data is binary encoded but
            // control information is not.
            int position = 0;
            int index;
            int shortBlockLength;
            char dataBlock;
            BitVector bitStream = new BitVector();

            do
            {
                if (binaryString.Length > 128)
                    throw new InvalidDataLengthException("Micro QR: Input data too long.");

                dataBlock = mode[position];
                shortBlockLength = 0;
                do
                {
                    shortBlockLength++;
                } while (((shortBlockLength + position) < length) && (mode[position + shortBlockLength] == dataBlock));

                switch (dataBlock)
                {
                    case 'K':   // Kanji mode.
                        // Mode indicator.
                        kanjiUsed = true;
                        binaryString.Append("K");
                        // Character count indicator.
                        binaryString.Append((char)shortBlockLength);
                        // Character representation.
                        for (int i = 0; i < shortBlockLength; i++)
                        {
                            int jisValue = (int)(jisData[position + i]);

                            if (jisValue >= 0x8140 && jisValue <= 0x9ffc)
                                jisValue -= 0x8140;

                            else if (jisValue >= 0xe040 && jisValue <= 0xebbf)
                                jisValue -= 0xc140;

                            int product = ((jisValue >> 8) * 0xc0) + (jisValue & 0xff);

                            bitStream.AppendBits(product, 13);
                            binaryString.Append(bitStream.ToString());
                            bitStream.Clear();
                            if (binaryString.Length > 128)
                                throw new InvalidDataLengthException("Micro QR: Input data too long.");
                        }
                        break;

                    case 'B':   // Byte mode.
                        // Mode indicator.
                        byteUsed = true;
                        binaryString.Append("B");
                        // Character count indicator.
                        binaryString.Append((char)shortBlockLength);
                        // Character representation.
                        for (int i = 0; i < shortBlockLength; i++)
                        {
                            int byteValue = jisData[position + i];
                            bitStream.AppendBits(byteValue, 8);
                            binaryString.Append(bitStream.ToString());
                            bitStream.Clear();
                            if (binaryString.Length > 128)
                                throw new InvalidDataLengthException("Micro QR: Input data too long.");
                        }
                        break;

                    case 'A':   // Alphanumeric mode.
                        // Mode indicator.
                        alphanumUsed = true;
                        binaryString.Append("A");
                        // Character count indicator.
                        binaryString.Append((char)shortBlockLength);
                        // Character representation.
                        index = 0;
                        while (index < shortBlockLength)
                        {
                            int count;
                            int first = 0, second = 0, product;

                            first = CharacterSets.AlphaNumericSet.IndexOf(jisData[position + index]);
                            count = 1;
                            product = first;

                            if (index + 1 < shortBlockLength && mode[position + index + 1] == 'A')
                            {
                                second = CharacterSets.AlphaNumericSet.IndexOf(jisData[position + index + 1]);
                                count = 2;
                                product = (first * 45) + second;
                            }

                            bitStream.AppendBits(product, count == 2 ? 11 : 6); // Count = 1..2 
                            binaryString.Append(bitStream.ToString());
                            bitStream.Clear();
                            if (binaryString.Length > 128)
                                throw new InvalidDataLengthException("Micro QR: Input data too long.");
                            index += 2;
                        };
                        break;

                    case 'N':   // Numeric mode.
                        // Mode indicator.
                        binaryString.Append("N");
                        // Character count indicator.
                        binaryString.Append((char)shortBlockLength);
                        // Character representation.
                        index = 0;
                        while (index < shortBlockLength)
                        {
                            int count;
                            int first = 0, second = 0, third = 0, product;

                            first = CharacterSets.NumberOnlySet.IndexOf(jisData[position + index]);
                            count = 1;
                            product = first;

                            if (index + 1 < shortBlockLength && mode[position + index + 1] == 'N')
                            {
                                second = CharacterSets.AlphaNumericSet.IndexOf(jisData[position + index + 1]);
                                count = 2;
                                product = (product * 10) + second;
                            }

                            if (index + 2 < shortBlockLength && mode[position + index + 2] == 'N')
                            {
                                third = CharacterSets.AlphaNumericSet.IndexOf(jisData[position + index + 2]);
                                count = 3;
                                product = (product * 10) + third;
                            }

                            bitStream.AppendBits(product, (3 * count) + 1);
                            binaryString.Append(bitStream.ToString());
                            bitStream.Clear();
                            if (binaryString.Length > 128)
                                throw new InvalidDataLengthException("Micro QR: Input data too long.");

                            index += 3;
                        };
                        break;
                }

                position += shortBlockLength;
            } while (position < length - 1);
        }

        private static void GetBitLength(int[] count, char[] binaryStream)
        {
            int index = 0;
            int length = binaryStream.Length;

            for (int i = 0; i < 4; i++)
                count[i] = 0;

            do
            {
                if ((binaryStream[index] == '0') || (binaryStream[index] == '1'))
                {
                    count[0]++;
                    count[1]++;
                    count[2]++;
                    count[3]++;
                    index++;
                }

                else
                {
                    switch (binaryStream[index])
                    {
                        case 'K':
                            count[2] += 5;
                            count[3] += 7;
                            index += 2;
                            break;

                        case 'B':
                            count[2] += 6;
                            count[3] += 8;
                            index += 2;
                            break;

                        case 'A':
                            count[1] += 4;
                            count[2] += 6;
                            count[3] += 8;
                            index += 2;
                            break;

                        case 'N':
                            count[0] += 3;
                            count[1] += 5;
                            count[2] += 7;
                            count[3] += 9;
                            index += 2;
                            break;
                    }
                }
            } while (index < length);
        }

        /// <summary>
        /// Encodes the input data into a binary stream.
        /// </summary>
        /// <param name="binaryStream"></param>
        /// <param name="bitStream"></param>
        /// <param name="version"></param>
        private static void MicroQRExpandBinary(char[] binaryStream, BitVector bitStream, int version)
        {
            int length = binaryStream.Length;
            int index = 0;
            do
            {
                switch (binaryStream[index])
                {
                    case '1':
                        bitStream.AppendBit(1);
                        index++;
                        break;

                    case '0':
                        bitStream.AppendBit(0);
                        index++;
                        break;

                    case 'N':   // Numeric Mode
                        // Mode indicator.
                        bitStream.AppendBits(0, version);
                        // Character count indicator.
                        bitStream.AppendBits(binaryStream[index + 1], 3 + version);     // version = 0..3
                        index += 2;
                        break;

                    case 'A':   // Alphanumeric Mode.
                        // Mode indicator.
                        bitStream.AppendBits(1, version);
                        // Character count indicator.
                        bitStream.AppendBits(binaryStream[index + 1], 2 + version);     // version = 1..3
                        index += 2;
                        break;

                    case 'B':   // Byte Mode.
                        // Mode indicator.
                        bitStream.AppendBits(2, version);
                        // Character count indicator.
                        bitStream.AppendBits(binaryStream[index + 1], 2 + version);     // version = 2..3
                        index += 2;
                        break;

                    case 'K':   // Kanji Mode.
                        // Mode indicator.
                        bitStream.AppendBits(3, version);
                        // Character count indicator.
                        bitStream.AppendBits(binaryStream[index + 1], 1 + version); //????
                        index += 2;
                        break;
                }

            } while (index < length);
        }

        private static void MicroQRM1(BitVector bitStream)
        {
            int remainder;
            int dataCodewords;
            int eccCodewords;
            byte[] dataBlocks;
            byte[] eccBlocks;

            int bitsTotal = 20;
            bool done = false;

            // Add terminator.
            int bitsLeft = bitsTotal - bitStream.SizeInBits;
            if (bitsLeft <= 3)
            {
                bitStream.AppendBits(0, bitsLeft);
                done = true;
            }

            else
                bitStream.AppendBits(0, 3);

            if (!done)
            {
                // Manage last (4-bit) block.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                if (bitsLeft <= 4)
                {
                    bitStream.AppendBits(0, bitsLeft);
                    done = true;
                }
            }

            if (!done)
            {
                // Complete current byte.
                remainder = 8 - (bitStream.SizeInBits % 8);
                if (remainder == 8)
                    remainder = 0;

                bitStream.AppendBits(0, remainder);

                // Add padding.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                if (bitsLeft > 4)
                {
                    remainder = (bitsLeft - 4) / 8;
                    for (int i = 0; i < remainder; i++)
                    {
                        if ((i & 1) != 0)
                            bitStream.AppendBits(0x11, 8);

                        else
                            bitStream.AppendBits(0xec, 8);
                    }
                }

                bitStream.AppendBits(0, 4);
            }

            dataCodewords = 3;
            eccCodewords = 2;
            dataBlocks = bitStream.ToByteArray();
            eccBlocks = new byte[eccCodewords];

            // Calculate Reed-Solomon error correction codewords.
            ReedSolomon.RSInitialise(0x11d, eccCodewords, 0);
            ReedSolomon.RSEncode(dataCodewords, dataBlocks, eccBlocks);

            // Add Reed-Solomon codewords to binary data.
            for (int i = 0; i < eccCodewords; i++)
                bitStream.AppendBits(eccBlocks[eccCodewords - i - 1], 8);
        }

        private static void MicroQRM2(BitVector bitStream, QRCodeErrorLevel eccLevel)
        {
            int bitsLeft, remainder;
            int dataCodewords = 0;
            int eccCodewords = 0;
            byte[] dataBlocks;
            byte[] eccBlocks;
            bool done = false;
            int bitsTotal = 0;

            if (eccLevel == QRCodeErrorLevel.Low)
                bitsTotal = 40;

            if (eccLevel == QRCodeErrorLevel.Medium)
                bitsTotal = 32;

            // Add terminator.
            bitsLeft = bitsTotal - bitStream.SizeInBits;
            if (bitsLeft <= 5)
            {
                bitStream.AppendBits(0, bitsLeft);
                done = true;
            }

            else
                bitStream.AppendBits(0, 5);


            if (!done)
            {
                // Complete current byte.
                remainder = 8 - (bitStream.SizeInBits % 8);
                if (remainder == 8)
                    remainder = 0;

                bitStream.AppendBits(0, remainder);

                // Add padding.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                remainder = bitsLeft / 8;
                for (int i = 0; i < remainder; i++)
                {
                    if ((i & 1) != 0)
                        bitStream.AppendBits(0x11, 8);

                    else
                        bitStream.AppendBits(0xec, 8);
                }
            }

            if (eccLevel == QRCodeErrorLevel.Low)
            {
                dataCodewords = 5;
                eccCodewords = 5;
            }

            if (eccLevel == QRCodeErrorLevel.Medium)
            {
                dataCodewords = 4;
                eccCodewords = 6;
            }

            dataBlocks = bitStream.ToByteArray();
            eccBlocks = new byte[eccCodewords];

            // Calculate Reed-Solomon error codewords.
            ReedSolomon.RSInitialise(0x11d, eccCodewords, 0);
            ReedSolomon.RSEncode(dataCodewords, dataBlocks, eccBlocks);

            // Add Reed-Solomon codewords to binary data.
            for (int i = 0; i < eccCodewords; i++)
                bitStream.AppendBits(eccBlocks[eccCodewords - i - 1], 8);
        }

        private static void MicroQRM3(BitVector bitStream, QRCodeErrorLevel eccLevel)
        {
            int bitsLeft;
            int remainder;
            byte[] dataBlocks;
            byte[] eccBlocks;

            int bitsTotal = 0;
            int dataCodewords = 0;
            int eccCodewords = 0;
            bool done = false;

            if (eccLevel == QRCodeErrorLevel.Low)
                bitsTotal = 84;

            if (eccLevel == QRCodeErrorLevel.Medium)
                bitsTotal = 68;

            // Add terminator.
            bitsLeft = bitsTotal - bitStream.SizeInBits;
            if (bitsLeft <= 7)
            {
                bitStream.AppendBits(0, bitsLeft);
                done = true;
            }

            else
                bitStream.AppendBits(0, 7);

            if (!done)
            {
                // Manage last (4-bit) block.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                if (bitsLeft <= 4)
                {
                    bitStream.AppendBits(0, bitsLeft);
                    done = true;
                }
            }

            if (!done)
            {
                // Complete current byte.
                remainder = 8 - (bitStream.SizeInBits % 8);
                if (remainder == 8)
                    remainder = 0;

                bitStream.AppendBits(0, remainder);

                // Add padding.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                if (bitsLeft > 4)
                {
                    remainder = (bitsLeft - 4) / 8;
                    for (int i = 0; i < remainder; i++)
                    {
                        if ((i & 1) != 0)
                            bitStream.AppendBits(0x11, 8);

                        else
                            bitStream.AppendBits(0xec, 8);
                    }
                }

                bitStream.AppendBits(0, 4);
            }

            if (eccLevel == QRCodeErrorLevel.Low)
            {
                dataCodewords = 11;
                eccCodewords = 6;
            }

            if (eccLevel == QRCodeErrorLevel.Medium)
            {
                dataCodewords = 9;
                eccCodewords = 8;
            }

            dataBlocks = bitStream.ToByteArray();
            eccBlocks = new byte[eccCodewords];

            if (eccLevel == QRCodeErrorLevel.Low)
                dataBlocks[10] = (byte)(dataBlocks[10] >> 4);

            if (eccLevel == QRCodeErrorLevel.Medium)
                dataBlocks[8] = (byte)(dataBlocks[8] >> 4);

            // Calculate Reed-Solomon error codewords.
            ReedSolomon.RSInitialise(0x11d, eccCodewords, 0);
            ReedSolomon.RSEncode(dataCodewords, dataBlocks, eccBlocks);

            // Add Reed-Solomon codewords to binary data.
            for (int i = 0; i < eccCodewords; i++)
                bitStream.AppendBits(eccBlocks[eccCodewords - i - 1], 8);
        }

        private static void MicroQRM4(BitVector bitStream, QRCodeErrorLevel eccLevel)
        {
            int bitsLeft;
            int remainder;
            int bitsTotal = 0;
            int dataCodewords = 0;
            int eccCodewords = 0;
            byte[] dataBlocks;
            byte[] eccBlocks;
            bool done = false;

            if (eccLevel == QRCodeErrorLevel.Low)
                bitsTotal = 128;

            if (eccLevel == QRCodeErrorLevel.Medium)
                bitsTotal = 112;

            if (eccLevel == QRCodeErrorLevel.Quartile)
                bitsTotal = 80;

            // Add terminator.
            bitsLeft = bitsTotal - bitStream.SizeInBits;
            if (bitsLeft <= 9)
            {
                bitStream.AppendBits(0, bitsLeft);
                done = true;
            }

            else
                bitStream.AppendBits(0, 9);

            if (!done)
            {
                // Complete current byte.
                remainder = 8 - (bitStream.SizeInBits % 8);
                if (remainder == 8)
                    remainder = 0;

                bitStream.AppendBits(0, remainder);

                // Add padding.
                bitsLeft = bitsTotal - bitStream.SizeInBits;
                remainder = bitsLeft / 8;
                for (int i = 0; i < remainder; i++)
                {
                    if ((i & 1) != 0)
                        bitStream.AppendBits(0x11, 8);

                    else
                        bitStream.AppendBits(0xec, 8);
                }
            }

            if (eccLevel == QRCodeErrorLevel.Low)
            {
                dataCodewords = 16;
                eccCodewords = 8;
            }

            if (eccLevel == QRCodeErrorLevel.Medium)
            {
                dataCodewords = 14;
                eccCodewords = 10;
            }

            if (eccLevel == QRCodeErrorLevel.Quartile)
            {
                dataCodewords = 10;
                eccCodewords = 14;
            }

            dataBlocks = bitStream.ToByteArray();
            eccBlocks = new byte[eccCodewords];

            // Calculate Reed-Solomon error codewords.
            ReedSolomon.RSInitialise(0x11d, eccCodewords, 0);
            ReedSolomon.RSEncode(dataCodewords, dataBlocks, eccBlocks);

            // Add Reed-Solomon codewords to binary data.
            for (int i = 0; i < eccCodewords; i++)
                bitStream.AppendBits(eccBlocks[eccCodewords - i - 1], 8);
        }

        private static void MicroSetupGrid(byte[] symbolGrid, int symbolSize)
        {
            bool latch = true; ;

            // Add timing patterns.
            for (int i = 0; i < symbolSize; i++)
            {
                if (latch)
                {
                    symbolGrid[i] = 0x21;
                    symbolGrid[(i * symbolSize)] = 0x21;
                    latch = false;
                }

                else
                {
                    symbolGrid[i] = 0x20;
                    symbolGrid[(i * symbolSize)] = 0x20;
                    latch = true;
                }
            }

            // Add finder patterns.
            PlaceFinderPatterns(symbolGrid, symbolSize, 0, 0);

            // Add separators.
            for (int i = 0; i < 7; i++)
            {
                symbolGrid[(7 * symbolSize) + i] = 0x10;
                symbolGrid[(i * symbolSize) + 7] = 0x10;
            }

            symbolGrid[(7 * symbolSize) + 7] = 0x10;


            // Reserve space for format information.
            for (int i = 0; i < 8; i++)
            {
                symbolGrid[(8 * symbolSize) + i] += 0x20;
                symbolGrid[(i * symbolSize) + 8] += 0x20;
            }

            symbolGrid[(8 * symbolSize) + 8] += 20;
        }

        private static void MicroPopulateGrid(byte[] symbolGrid, int symbolSize, BitVector bitStream)
        {
            int direction = 1;  // Up.
            int row = 0;        // Right hand side.

            int n = bitStream.SizeInBits;
            int y = symbolSize - 1;
            int i = 0;
            int x;
            do
            {
                x = (symbolSize - 2) - (row * 2);

                if ((symbolGrid[(y * symbolSize) + (x + 1)] & 0xf0) == 0)
                {
                    if (bitStream[i] == 1)
                        symbolGrid[(y * symbolSize) + (x + 1)] = 0x01;

                    else
                        symbolGrid[(y * symbolSize) + (x + 1)] = 0x00;

                    i++;
                }

                if (i < n)
                {
                    if ((symbolGrid[(y * symbolSize) + x] & 0xf0) == 0)
                    {
                        if (bitStream[i] == 1)
                            symbolGrid[(y * symbolSize) + x] = 0x01;

                        else
                            symbolGrid[(y * symbolSize) + x] = 0x00;

                        i++;
                    }
                }

                if (direction == 1)
                    y--;

                else
                    y++;

                if (y == 0)
                {
                    // Reached the top.
                    row++;
                    y = 1;
                    direction = 0;
                }

                if (y == symbolSize)
                {
                    // Reached the bottom.
                    row++;
                    y = symbolSize - 1;
                    direction = 1;
                }
            }
            while (i < n);
        }

        private static int MicroEvaluate(byte[] data, int symbolSize, int pattern)
        {
            byte filter = 0;
            int result = 0;

            switch (pattern)
            {
                case 0:
                    filter = 0x01;
                    break;

                case 1:
                    filter = 0x02;
                    break;

                case 2:
                    filter = 0x04;
                    break;

                case 3:
                    filter = 0x08;
                    break;
            }

            int sum1 = 0;
            int sum2 = 0;
            for (int i = 1; i < symbolSize; i++)
            {
                if ((data[(i * symbolSize) + symbolSize - 1] & filter) != 0)
                    sum1++;

                if ((data[((symbolSize - 1) * symbolSize) + i] & filter) != 0)
                    sum2++;
            }

            if (sum1 <= sum2)
                result = (sum1 * 16) + sum2;

            else
                result = (sum2 * 16) + sum1;

            return result;
        }

        private static int MicroApplyBitmask(byte[] symbolGrid, int symbolSize)
        {
            byte p;
            int pattern;
            int[] value = new int[8];
            int bestValue, bestPattern;
            int bit;

            byte[] mask = new byte[symbolSize * symbolSize];
            byte[] Evaluate = new byte[symbolSize * symbolSize];

            // Perform data masking.
            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    mask[(y * symbolSize) + x] = 0x00;

                    if ((symbolGrid[(y * symbolSize) + x] & 0xf0) == 0)
                    {
                        if ((y & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x01;

                        if ((((y / 2) + (x / 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x02;

                        if (((((y * x) & 1) + ((y * x) % 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x04;

                        if (((((y + x) & 1) + ((y * x) % 3)) & 1) == 0)
                            mask[(y * symbolSize) + x] += 0x08;
                    }
                }
            }

            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    if ((symbolGrid[(y * symbolSize) + x] & 0x01) != 0)
                        p = 0xff;

                    else
                        p = 0x00;

                    Evaluate[(y * symbolSize) + x] = (byte)(mask[(y * symbolSize) + x] ^ p);
                }
            }


            // Evaluate result.
            for (pattern = 0; pattern < 8; pattern++)
                value[pattern] = MicroEvaluate(Evaluate, symbolSize, pattern);

            bestPattern = 0;
            bestValue = value[0];
            for (pattern = 1; pattern < 4; pattern++)
            {
                if (value[pattern] > bestValue)
                {
                    bestPattern = pattern;
                    bestValue = value[pattern];
                }
            }

            // Apply mask.
            for (int x = 0; x < symbolSize; x++)
            {
                for (int y = 0; y < symbolSize; y++)
                {
                    bit = 0;
                    switch (bestPattern)
                    {
                        case 0:
                            if ((mask[(y * symbolSize) + x] & 0x01) != 0)
                                bit = 1;

                            break;

                        case 1:
                            if ((mask[(y * symbolSize) + x] & 0x02) != 0)
                                bit = 1;

                            break;

                        case 2:
                            if ((mask[(y * symbolSize) + x] & 0x04) != 0)
                                bit = 1;

                            break;

                        case 3:
                            if ((mask[(y * symbolSize) + x] & 0x08) != 0)
                                bit = 1;

                            break;
                    }

                    if (bit == 1)
                    {
                        if ((symbolGrid[(y * symbolSize) + x] & 0x01) != 0)
                            symbolGrid[(y * symbolSize) + x] = 0;

                        else
                            symbolGrid[(y * symbolSize) + x] = 1;
                    }
                }
            }

            return bestPattern;
        }

        private void UPNQR()
        {
            /* Characteristics of the printed QR code on the UPN QR form 
             a.  Version QR: Version 15 (77x77 modules).  Version 15 is required, regardless of the amount of data entered. 
             b.  Data type QR: Byte data (Binary, binary). 
             c.  Data Correction Rate QR: ECC_M (Error Correction Level M). 
             d.  Character set QR: ISO 8859-2.  It is mandatory to use Extended Channel Interpretation (ECI value 000004). 
             e.  Module size QR: 0.42333 mm x 0.42333 mm. 
             f.  Size of QR code with mandatory white margin: 32.59676 x 32.59667 mm 
             g.  Space occupied by QR with mandatory white edge: 35.98333 x 35.98333 mm. 
             h.  Minimum resolution of QR output: 600 x 600 DPI. 

             If we fulfill the QR code, the limits for the length of the individual fields are in accordance with the QR structure. 
             The content of the QR record must be identical to the record on the form. */
            int version;
            QRCodeErrorLevel eccLevel;
            int symbolSize;
            int estimatedBinaryLength;
            BitVector bitStream;
            int inputLength = barcodeData.Length;
            char[] upnData = new char[inputLength];
            char[] mode = new char[inputLength];

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] <= 0xff)
                {
                    upnData[i] = barcodeData[i];
                    mode[i] = 'B';
                }

                else
                    throw new InvalidDataException("UPN QR Code: Invalid data in input.");
            }

            eci = 4;
            version = 15;
            eccLevel = QRCodeErrorLevel.Medium;

            estimatedBinaryLength = GetBinaryLength(version, mode, upnData, inputLength);
            if (estimatedBinaryLength > 3320)
                throw new InvalidDataLengthException("UPN QR Code: Input data too long.");

            int dataCodewords = QRDataCodeWordsMedium[version - 1];
            int blocks = QRBlocksMedium[version - 1];

            bitStream = new BitVector(QRTotalCodeWords[version - 1]);
            QRBinary(bitStream, mode, upnData, version, dataCodewords);
            AddErrorCorrection(bitStream, version, dataCodewords, blocks);

            symbolSize = QRSizes[version - 1];
            byte[] symbolGrid = new byte[symbolSize * symbolSize];
            SetupGrid(symbolGrid, symbolSize, version);
            PopulateGrid(symbolGrid, symbolSize, bitStream);
            AddVersionInformation(symbolGrid, symbolSize, version);

            int bitMask = ApplyBitmask(symbolGrid, symbolSize, eccLevel);
            AddFormatInformationGrid(symbolGrid, symbolSize, eccLevel, bitMask);
            BuildSymbol(symbolGrid, symbolSize);
        }
    }
}