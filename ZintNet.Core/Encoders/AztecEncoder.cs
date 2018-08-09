/* AztecEncoder.cs - Handles encoding of Aztec & Aztec Runes 2D symbols */

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
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal class AztecEncoder : SymbolEncoder
    {
        #region Tables and Constants
        static int[] CompactAztecMap = {
            // 27 x 27 data grid.
	        609,608,411,413,415,417,419,421,423,425,427,429,431,433,435,437,439,441,443,445,447,449,451,453,455,457,459,
	        607,606,410,412,414,416,418,420,422,424,426,428,430,432,434,436,438,440,442,444,446,448,450,452,454,456,458,
	        605,604,409,408,243,245,247,249,251,253,255,257,259,261,263,265,267,269,271,273,275,277,279,281,283,460,461,
	        603,602,407,406,242,244,246,248,250,252,254,256,258,260,262,264,266,268,270,272,274,276,278,280,282,462,463,
	        601,600,405,404,241,240,107,109,111,113,115,117,119,121,123,125,127,129,131,133,135,137,139,284,285,464,465,
	        599,598,403,402,239,238,106,108,110,112,114,116,118,120,122,124,126,128,130,132,134,136,138,286,287,466,467,
	        597,596,401,400,237,236,105,104,3,5,7,9,11,13,15,17,19,21,23,25,27,140,141,288,289,468,469,
	        595,594,399,398,235,234,103,102,2,4,6,8,10,12,14,16,18,20,22,24,26,142,143,290,291,470,471,
	        593,592,397,396,233,232,101,100,1,1,2000,2001,2002,2003,2004,2005,2006,0,1,28,29,144,145,292,293,472,473,
	        591,590,395,394,231,230,99,98,1,1,1,1,1,1,1,1,1,1,1,30,31,146,147,294,295,474,475,
	        589,588,393,392,229,228,97,96,2027,1,0,0,0,0,0,0,0,1,2007,32,33,148,149,296,297,476,477,
	        587,586,391,390,227,226,95,94,2026,1,0,1,1,1,1,1,0,1,2008,34,35,150,151,298,299,478,479,
	        585,584,389,388,225,224,93,92,2025,1,0,1,0,0,0,1,0,1,2009,36,37,152,153,300,301,480,481,
	        583,582,387,386,223,222,91,90,2024,1,0,1,0,1,0,1,0,1,2010,38,39,154,155,302,303,482,483,
	        581,580,385,384,221,220,89,88,2023,1,0,1,0,0,0,1,0,1,2011,40,41,156,157,304,305,484,485,
	        579,578,383,382,219,218,87,86,2022,1,0,1,1,1,1,1,0,1,2012,42,43,158,159,306,307,486,487,
	        577,576,381,380,217,216,85,84,2021,1,0,0,0,0,0,0,0,1,2013,44,45,160,161,308,309,488,489,
	        575,574,379,378,215,214,83,82,0,1,1,1,1,1,1,1,1,1,1,46,47,162,163,310,311,490,491,
	        573,572,377,376,213,212,81,80,0,0,2020,2019,2018,2017,2016,2015,2014,0,0,48,49,164,165,312,313,492,493,
	        571,570,375,374,211,210,78,76,74,72,70,68,66,64,62,60,58,56,54,50,51,166,167,314,315,494,495,
	        569,568,373,372,209,208,79,77,75,73,71,69,67,65,63,61,59,57,55,52,53,168,169,316,317,496,497,
	        567,566,371,370,206,204,202,200,198,196,194,192,190,188,186,184,182,180,178,176,174,170,171,318,319,498,499,
	        565,564,369,368,207,205,203,201,199,197,195,193,191,189,187,185,183,181,179,177,175,172,173,320,321,500,501,
	        563,562,366,364,362,360,358,356,354,352,350,348,346,344,342,340,338,336,334,332,330,328,326,322,323,502,503,
	        561,560,367,365,363,361,359,357,355,353,351,349,347,345,343,341,339,337,335,333,331,329,327,324,325,504,505,
	        558,556,554,552,550,548,546,544,542,540,538,536,534,532,530,528,526,524,522,520,518,516,514,512,510,506,507,
	        559,557,555,553,551,549,547,545,543,541,539,537,535,533,531,529,527,525,523,521,519,517,515,513,511,508,509};

        static int[] AztecSymbolCharacter = {
            // From Table 2.
	        0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 300, 14, 15, 16, 17, 18, 19,
	        20, 21, 22, 23, 24, 25, 26, 15, 16, 17, 18, 19, 1, 6, 7, 8, 9, 10, 11, 12,
	        13, 14, 15, 16, 301, 18, 302, 20, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 21, 22,
	        23, 24, 25, 26, 20, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
	        17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 27, 21, 28, 22, 23, 24, 2, 3, 4,
	        5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
	        25, 26, 27, 29, 25, 30, 26, 27};

        static string AztecModes = "BMMMMMMMMMMMMXBBBBBBBBBBBBBMMMMMXPPPPPPPPPPPXPXPDDDDDDDDDDPPPPPPMUUUUUUUUUUUUUUUUUUUUUUUUUUPMPMMMLLLLLLLLLLLLLLLLLLLLLLLLLLPMPMM";

        static int[] AztecSizes = {
            // Codewords per symbol.
	        21, 48, 60, 88, 120, 156, 196, 240, 230, 272, 316, 364, 416, 470, 528, 588, 652, 720, 790,
	        864, 940, 1020, 920, 992, 1066, 1144, 1224, 1306, 1392, 1480, 1570, 1664};

        static int[] AztecCompactSizes = { 17, 40, 51, 76 };

        static int[] Aztec10DataSizes = {
            // Data bits per symbol maximum with 10% error correction.
            96, 246, 408, 616, 840, 1104, 1392, 1704, 2040, 2420, 2820, 3250, 3720, 4200, 4730,
            5270, 5840, 6450, 7080, 7750, 8430, 9150, 9900, 10680, 11484, 12324, 13188, 14076,
            15000, 15948, 16920, 17940};

        static int[] Aztec23DataSizes = {
            // Data bits per symbol maximum with 23% error correction.
	        84, 204, 352, 520, 720, 944, 1184, 1456, 1750, 2070, 2410, 2780, 3180, 3590, 4040,
	        4500, 5000, 5520, 6060, 6630, 7210, 7830, 8472, 9132, 9816, 10536, 11280, 12036,
	        12828, 13644, 14472, 15348};

        static int[] Aztec36DataSizes = {
            // Data bits per symbol maximum with 36% error correction.
	        66, 168, 288, 432, 592, 776, 984, 1208, 1450, 1720, 2000, 2300, 2640, 2980, 3350,
	        3740, 4150, 4580, 5030, 5500, 5990, 6500, 7032, 7584, 8160, 8760, 9372, 9996, 10656,
	        11340, 12024, 12744};

        static int[] Aztec50DataSizes = {
            // Data bits per symbol maximum with 50% error correction.
	        48, 126, 216, 328, 456, 600, 760, 936, 1120, 1330, 1550, 1790, 2050, 2320, 2610,
	        2910, 3230, 3570, 3920, 4290, 4670, 5070, 5484, 5916, 6360, 6828, 7308, 7800, 8316,
	        8844, 9384, 9948};

        static int[] AztecCompact10DataSizes = { 78, 198, 336, 520 };
        static int[] AztecCompact23DataSizes = { 66, 168, 288, 440 };
        static int[] AztecCompact36DataSizes = { 48, 138, 232, 360 };
        static int[] AztecCompact50DataSizes = { 36, 102, 176, 280 };

        static int[] AztecOffset = {
            66, 64, 62, 60, 57, 55, 53, 51, 49, 47, 45, 42, 40, 38, 36, 34, 32, 30, 28, 25, 23, 21,
            19, 17, 15, 13, 10, 8, 6, 4, 2, 0};

        static int[] AztecCompactOffset = { 6, 4, 2, 0 };

        static int[] AztecMap = new int[22801];

        // Modes.
        const int UPPER = 1;
        const int LOWER = 2;
        const int MIXED = 4;
        const int PUNC = 8;
        const int DIGIT = 16;
        const int BINARY = 32;

        const int CompactLoop = 4;
        #endregion

        private int optionErrorCorrection;
        private int optionSymbolSize;


        public AztecEncoder(Symbology symbology, string barcodeMessage, int optionSymbolSize, int optionErrorCorrection, int eci, EncodingMode mode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionErrorCorrection = optionErrorCorrection;
            this.optionSymbolSize = optionSymbolSize;
            this.eci = eci;
            this.encodingMode = mode;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.Aztec:
                    switch (encodingMode)
                    {
                        case EncodingMode.Standard:
                            isGS1 = false;
                            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
                            Aztec();
                            break;

                        case EncodingMode.GS1:
                            isGS1 = true;
                            barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                            Aztec();
                            break;

                        case EncodingMode.HIBC:
                            isGS1 = false;
                            barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                            Aztec();
                            break;
                    }
                    break;

                case Symbology.AztecRunes:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    AztecRunes();
                    break;
            }

            return Symbol;
        }

        /// <summary>
        /// Generates an Aztec (ISO 24778) symbol.
        /// </summary>
        private void Aztec()
        {
            int dataBlocks, eccBlocks;
            int eccLevel;
            bool isCompactSymbol = false;
            int adjustmentSize = 0;
            int layers = 0;
            int dataMaxSize = 0;
            int codewordBitSize = 0;
            int bitStreamLength = 0;
            BitVector bitStream = new BitVector();
            BitVector bitPattern = new BitVector();
            byte[] dataDescriptor = new byte[4];
            byte[] eccDescriptor = new byte[6];
            byte[] descriptor = new byte[40];

            int inputLength = barcodeData.Length;

            PopulateMap();
            if (!ProcessAztecText(bitStream, inputLength))
                throw new InvalidDataLengthException("Aztec Code: Input data too long or too many extended ASCII characters.");

            bitStreamLength = bitStream.SizeInBits;
            eccLevel = optionErrorCorrection;
            if (optionErrorCorrection == -1)
                eccLevel = 2;

            if (optionSymbolSize == 0)    // Auto sizing.
            {
                do
                {
                    // Decide what size symbol to use - the smallest that fits the data.
                    isCompactSymbol = false;
                    layers = 0;

                    switch (eccLevel)
                    {
                        // For each level of error correction work out the smallest symbol which the data will fit into.
                        case 1:
                            for (int i = 32; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < Aztec10DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = false;
                                    dataMaxSize = Aztec10DataSizes[i - 1];
                                }
                            }

                            for (int i = CompactLoop; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < AztecCompact10DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = true;
                                    dataMaxSize = AztecCompact10DataSizes[i - 1];
                                }
                            }
                            break;

                        case 2:
                            for (int i = 32; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < Aztec23DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = false;
                                    dataMaxSize = Aztec23DataSizes[i - 1];
                                }
                            }

                            for (int i = CompactLoop; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < AztecCompact23DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = true;
                                    dataMaxSize = AztecCompact23DataSizes[i - 1];
                                }
                            }
                            break;

                        case 3:
                            for (int i = 32; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < Aztec36DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = false;
                                    dataMaxSize = Aztec36DataSizes[i - 1];
                                }
                            }

                            for (int i = CompactLoop; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < AztecCompact36DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = true;
                                    dataMaxSize = AztecCompact36DataSizes[i - 1];
                                }
                            }
                            break;

                        case 4:
                            for (int i = 32; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < Aztec50DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = false;
                                    dataMaxSize = Aztec50DataSizes[i - 1];
                                }
                            }

                            for (int i = CompactLoop; i > 0; i--)
                            {
                                if ((bitStreamLength + adjustmentSize) < AztecCompact50DataSizes[i - 1])
                                {
                                    layers = i;
                                    isCompactSymbol = true;
                                    dataMaxSize = AztecCompact50DataSizes[i - 1];
                                }
                            }
                            break;
                    }

                    if (layers == 0)    // Couldn't find a symbol which fits the data.
                        throw new InvalidDataLengthException("Aztec Code: Input data too long. (Too many bits for selected ECC)");

                    // Determine codeword bit length - Table 3.
                    codewordBitSize = 6;
                    if ((layers >= 3) && (layers <= 8))
                        codewordBitSize = 8;

                    if ((layers >= 9) && (layers <= 22))
                        codewordBitSize = 10;

                    if (layers >= 23)
                        codewordBitSize = 12;

                    AdjustBinaryData(bitStream, codewordBitSize);
                    bitStreamLength = bitStream.SizeInBits;

                } while (bitStreamLength > dataMaxSize);
                /* Note: This loop will only repeat on the rare occasions when the rule about not having all 1s or all 0s
                means that the binary string has had to be lengthened beyond the maximum number of bits that can
                be encoded in a symbol of the selected size.*/
            }

            else
            {
                // The size of the symbol has been specified by the user.
                if ((optionSymbolSize >= 1) && (optionSymbolSize <= 4))
                {
                    isCompactSymbol = true;
                    layers = optionSymbolSize;
                }

                if ((optionSymbolSize >= 5) && (optionSymbolSize <= 36))
                {
                    isCompactSymbol = false;
                    layers = optionSymbolSize - 4;
                }

                // Determine codeword bit length - Table 3.
                if ((layers >= 0) && (layers <= 2))
                    codewordBitSize = 6;

                if ((layers >= 3) && (layers <= 8))
                    codewordBitSize = 8;

                if ((layers >= 9) && (layers <= 22))
                    codewordBitSize = 10;

                if (layers >= 23)
                    codewordBitSize = 12;

                AdjustBinaryData(bitStream, codewordBitSize);
                bitStreamLength = bitStream.SizeInBits;

                // Check if the data actually fits into the selected symbol size.
                if (isCompactSymbol)
                    dataMaxSize = codewordBitSize * (AztecCompactSizes[layers - 1] - 3);

                else
                    dataMaxSize = codewordBitSize * (AztecSizes[layers - 1] - 3);

                if (bitStreamLength > dataMaxSize)
                    throw new InvalidDataLengthException("Aztec Code: Data too long for specified symbol size.");
            }

            dataBlocks = bitStreamLength / codewordBitSize;

            if (isCompactSymbol)
                eccBlocks = AztecCompactSizes[layers - 1] - dataBlocks;

            else
                eccBlocks = AztecSizes[layers - 1] - dataBlocks;

            uint[] dataCodewords = new uint[dataBlocks];
            uint[] eccCodewords = new uint[eccBlocks];

            // Split into codewords and calculate reed-solomon error correction codes.
            for (int i = 0; i < dataBlocks; i++)
            {
                for (int p = 0; p < codewordBitSize; p++)
                {
                    if (bitStream[i * codewordBitSize + p] == 1)
                        dataCodewords[i] += (uint)(0x01 << (codewordBitSize - (p + 1)));
                }
            }

            switch (codewordBitSize)
            {
                case 6:
                    ReedSolomon.RSInitialise(0x43, eccBlocks, 1);
                    ReedSolomon.RSEncode(dataBlocks, dataCodewords, eccCodewords);
                    for (int i = (eccBlocks - 1); i >= 0; i--)
                        bitStream.AppendBits((int)eccCodewords[i], 6);

                    break;

                case 8:
                    ReedSolomon.RSInitialise(0x12d, eccBlocks, 1);
                    ReedSolomon.RSEncode(dataBlocks, dataCodewords, eccCodewords);
                    for (int i = (eccBlocks - 1); i >= 0; i--)
                        bitStream.AppendBits((int)eccCodewords[i], 8);

                    break;

                case 10:
                    ReedSolomon.RSInitialise(0x409, eccBlocks, 1);
                    ReedSolomon.RSEncode(dataBlocks, dataCodewords, eccCodewords);
                    for (int i = (eccBlocks - 1); i >= 0; i--)
                        bitStream.AppendBits((int)eccCodewords[i], 10);

                    break;

                case 12:
                    ReedSolomon.RSInitialise(0x1069, eccBlocks, 1);
                    ReedSolomon.RSEncode(dataBlocks, dataCodewords, eccCodewords);
                    for (int i = (eccBlocks - 1); i >= 0; i--)
                        bitStream.AppendBits((int)eccCodewords[i], 12);

                    break;
            }

            int totalBits = (dataBlocks + eccBlocks) * codewordBitSize;
            // Check our encoding is correct.
            if (totalBits != bitStream.SizeInBits)
                throw new DataEncodingException("Aztec Code: Bitstream size error.");

            // Invert the data so that actual data is on the outside and reed-solomon on the inside.
            for (int i = 0; i < totalBits; i++)
                bitPattern.AppendBit(bitStream[totalBits - i - 1]);

            // Add the symbol descriptor.
            if (isCompactSymbol)
            {
                // The first 2 bits represent the number of layers minus 1.
                descriptor[0] = (((layers - 1) & 0x02) != 0) ? (byte)1 : (byte)0;
                descriptor[1] = (((layers - 1) & 0x01) != 0) ? (byte)1 : (byte)0;

                // The next 6 bits represent the number of data blocks minus 1.
                for (int x = 0; x < 6; x++)
                    descriptor[2 + x] = (((dataBlocks - 1) & (0x20 >> x)) != 0) ? (byte)1 : (byte)0;
            }

            else
            {
                // The first 5 bits represent the number of layers minus 1.
                for (int x = 0; x < 5; x++)
                    descriptor[x] = (((layers - 1) & (0x10 >> x)) != 0) ? (byte)1 : (byte)0;

                // The next 11 bits represent the number of data blocks minus 1.
                for (int x = 0; x < 11; x++)
                    descriptor[5 + x] = (((dataBlocks - 1) & (0x400 >> x)) != 0) ? (byte)1 : (byte)0;
            }

            // Split into 4 x 4-bit codewords.
            // Add reed-solomon error correction with Galois field 
            // GF(16) and prime modulus: x^4 + x + 1 (section 7.2.3)

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (descriptor[(i * 4) + j] == 1)
                        dataDescriptor[i] += (byte)(8 >> j);
                }
            }

            if (isCompactSymbol)
            {
                ReedSolomon.RSInitialise(0x13, 5, 1);
                ReedSolomon.RSEncode(2, dataDescriptor, eccDescriptor);
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                        descriptor[(i * 4) + 8 + j] = ((eccDescriptor[4 - i] & (0x08 >> j)) != 0) ? (byte)1 : (byte)0;
                }
            }

            else
            {
                ReedSolomon.RSInitialise(0x13, 6, 1);
                ReedSolomon.RSEncode(4, dataDescriptor, eccDescriptor);
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 4; j++)
                        descriptor[(i * 4) + 16 + j] = ((eccDescriptor[5 - i] & (0x08 >> j)) != 0) ? (byte)1 : (byte)0;
                }
            }

            // Reverse data and merge descriptor with the rest of the symbol.
            int descriptorOffest = isCompactSymbol ? 1998 : 19998;
            int offsetCount = descriptorOffest - bitStream.SizeInBits;
            for (int i = 0; i < offsetCount; i++)
                bitPattern.AppendBit(0);

            for (int i = 0; i < 40; i++)
                bitPattern.AppendBit((descriptor[i] == 1) ? (byte)1 : (byte)0);

            // Expand the row pattern into the symbol data.
            BuildSymbolData(bitPattern, layers, isCompactSymbol);
        }

        /// <summary>
        /// Adjusts the binary data and add padding.
        /// </summary>
        /// <param name="binaryData">bit stream</param>
        /// <param name="codewordBitSize">number of bits in the codeword</param>
        private static void AdjustBinaryData(BitVector binaryData, int codewordBitSize)
        {
            int length = binaryData.SizeInBits;
            int index = 0;
            int count;

            do
            {
                if ((index + 1) % codewordBitSize == 0) // Last bit of codeword.
                {
                    count = 0;
                    // Discover how many '1's in current codeword.
                    for (int i = 0; i < (codewordBitSize - 1); i++)
                    {
                        if (binaryData[(index - (codewordBitSize - 1)) + i] == 1)
                            count++;
                    }

                    if (count == (codewordBitSize - 1))
                        binaryData.Insert(index, 0);

                    else if (count == 0)
                        binaryData.Insert(index, 1);

                    else
                        index++;
                }
                
                index++;
            } while (index < length);

            // Add padding
            int binaryDataLength = binaryData.SizeInBits;
            int remainder = binaryDataLength % codewordBitSize;
            int padBits = codewordBitSize - remainder;

            if (padBits == codewordBitSize)
                padBits = 0;

            binaryData.AppendBits(0xffff, padBits);
            binaryDataLength += padBits;
            count = 0;
            for (int i = (binaryDataLength - codewordBitSize); i < binaryDataLength; i++)
            {
                if (binaryData[i] == 1)
                    count++;
            }

            if (count == codewordBitSize)
                binaryData[binaryDataLength - 1] = 0;
        }

        private bool ProcessAztecText(BitVector bitStream, int inputLength)
        {
            char[] encodeMode = new char[inputLength];
            char[] reducedSource = new char[inputLength];
            char[] reducedEncodeMode = new char[inputLength];
            char currentMode;
            int count;
            int l;
            int k;
            char nextMode;
            int reducedLength;
            bool byteMode = false;

            for (int i = 0; i < inputLength; i++)
            {
                if (barcodeData[i] > 127)
                    encodeMode[i] = 'B';

                else
                    encodeMode[i] = AztecModes[barcodeData[i]];
            }

            // Deal first with letter combinations which can be combined to one codeword
            // Combinations are (CR LF) (. SP) (, SP) (: SP) in Punct mode
            currentMode = 'U';
            for (int i = 0; i < inputLength - 1; i++)
            {
                // Combination (CR LF) should always be in Punct mode
                if ((barcodeData[i] == 13) && (barcodeData[i + 1] == 10))
                {
                    encodeMode[i] = 'P';
                    encodeMode[i + 1] = 'P';
                }

                // Combination (: SP) should always be in Punct mode
                if ((barcodeData[i] == ':') && (barcodeData[i + 1] == ' '))
                    encodeMode[i + 1] = 'P';

                // Combinations (. SP) and (, SP) sometimes use fewer bits in Digit mode
                if (((barcodeData[i] == '.') || (barcodeData[i] == ',')) && (barcodeData[i + 1] == ' ') && (encodeMode[i] == 'X'))
                {
                    count = CountDoubles(i, inputLength);
                    nextMode = GetNextMode(encodeMode, inputLength, i);

                    if (currentMode == 'U')
                    {
                        if ((nextMode == 'D') && (count <= 5))
                        {
                            for (int j = 0; j < (2 * count); j++)
                                encodeMode[i + j] = 'D';
                        }
                    }

                    if (currentMode == 'L')
                    {
                        if ((nextMode == 'U') && (count == 1))
                        {
                            encodeMode[i] = 'D';
                            encodeMode[i + 1] = 'D';
                        }

                        if ((nextMode == 'D') && (count <= 4))
                        {
                            for (int j = 0; j < (2 * count); j++)
                                encodeMode[i + j] = 'D';
                        }
                    }

                    if (currentMode == 'M')
                    {
                        if ((nextMode == 'D') && (count == 1))
                        {
                            encodeMode[i] = 'D';
                            encodeMode[i + 1] = 'D';
                        }
                    }

                    if (currentMode == 'D')
                    {
                        if ((nextMode != 'D') && (count <= 4))
                        {
                            for (int j = 0; j < (2 * count); j++)
                                encodeMode[i + j] = 'D';
                        }

                        if ((nextMode == 'D') && (count <= 7))
                        {
                            for (int j = 0; j < (2 * count); j++)
                                encodeMode[i + j] = 'D';
                        }
                    }

                    // Default is Punct mode
                    if (encodeMode[i] == 'X')
                    {
                        encodeMode[i] = 'P';
                        encodeMode[i + 1] = 'P';
                    }
                }

                if ((encodeMode[i] != 'X') && (encodeMode[i] != 'B'))
                    currentMode = encodeMode[i];
            }

            // Reduce two letter combinations to one codeword marked as [abcd] in Punct mode
            l = 0;
            k = 0;
            do
            {
                if (l < inputLength - 1)
                {
                    if ((barcodeData[l] == 13) && (barcodeData[l + 1] == 10))
                    { // CR LF
                        reducedSource[k] = 'a';
                        reducedEncodeMode[k] = encodeMode[l];
                        l += 2;
                    }

                    else if (((barcodeData[l] == '.') && (barcodeData[l + 1] == ' ')) && (encodeMode[l] == 'P'))
                    {
                        reducedSource[k] = 'b';
                        reducedEncodeMode[k] = encodeMode[l];
                        l += 2;
                    }

                    else if (((barcodeData[l] == ',') && (barcodeData[l + 1] == ' ')) && (encodeMode[l] == 'P'))
                    {
                        reducedSource[k] = 'c';
                        reducedEncodeMode[k] = encodeMode[l];
                        l += 2;
                    }

                    else if ((barcodeData[l] == ':') && (barcodeData[l + 1] == ' '))
                    {
                        reducedSource[k] = 'd';
                        reducedEncodeMode[k] = encodeMode[l];
                        l += 2;
                    }

                    else
                    {
                        reducedSource[k] = barcodeData[l];
                        reducedEncodeMode[k] = encodeMode[l];
                        l++;
                    }
                }

                else
                {
                    reducedSource[k] = barcodeData[l];
                    reducedEncodeMode[k] = encodeMode[l];
                    l++;
                }

                k++;
            } while (l < inputLength);

            reducedLength = k;

            currentMode = 'U';
            for (int i = 0; i < reducedLength; i++)
            {
                // Resolve Carriage Return (CR) which can be Punct or Mixed mode
                if (reducedSource[i] == 13)
                {
                    count = CountCR(reducedSource, i, reducedLength);
                    nextMode = GetNextMode(reducedEncodeMode, reducedLength, i);

                    if ((currentMode == 'U') && ((nextMode == 'U') || (nextMode == 'B')) && (count == 1))
                        reducedEncodeMode[i] = 'P';

                    if ((currentMode == 'L') && ((nextMode == 'L') || (nextMode == 'B')) && (count == 1))
                        reducedEncodeMode[i] = 'P';

                    if ((currentMode == 'P') || (nextMode == 'P'))
                        reducedEncodeMode[i] = 'P';

                    if (currentMode == 'D')
                    {
                        if (((nextMode == 'E') || (nextMode == 'U') || (nextMode == 'D') || (nextMode == 'B')) && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'P';
                        }

                        if ((nextMode == 'L') && (count == 1))
                            reducedEncodeMode[i] = 'P';
                    }

                    // Default is Mixed mode
                    if (reducedEncodeMode[i] == 'X')
                        reducedEncodeMode[i] = 'M';
                }

                // Resolve full stop and comma which can be in Punct or Digit mode
                if ((reducedSource[i] == '.') || (reducedSource[i] == ','))
                {
                    count = CountDotComma(reducedSource, i, reducedLength);
                    nextMode = GetNextMode(reducedEncodeMode, reducedLength, i);

                    if (currentMode == 'U')
                    {
                        if (((nextMode == 'U') || (nextMode == 'L') || (nextMode == 'M') || (nextMode == 'B')) && (count == 1))
                            reducedEncodeMode[i] = 'P';
                    }

                    if (currentMode == 'L')
                    {
                        if ((nextMode == 'L') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'P';
                        }

                        if (((nextMode == 'M') || (nextMode == 'B')) && (count == 1))
                            reducedEncodeMode[i] = 'P';
                    }

                    if (currentMode == 'M')
                    {
                        if (((nextMode == 'E') || (nextMode == 'U') || (nextMode == 'L') || (nextMode == 'M')) && (count <= 4))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'P';
                        }

                        if ((nextMode == 'B') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'P';
                        }
                    }

                    if ((currentMode == 'P') && (nextMode != 'D') && (count <= 9))
                    {
                        for (int j = 0; j < count; j++)
                            reducedEncodeMode[i + j] = 'P';
                    }

                    // Default is Digit mode
                    if (reducedEncodeMode[i] == 'X')
                        reducedEncodeMode[i] = 'D';
                }

                // Resolve Space (SP) which can be any mode except Punct
                if (reducedSource[i] == ' ')
                {
                    count = CountSpaces(reducedSource, i, reducedLength);
                    nextMode = GetNextMode(reducedEncodeMode, reducedLength, i);

                    if (currentMode == 'U')
                    {
                        if ((nextMode == 'E') && (count <= 5))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'U';
                        }

                        if (((nextMode == 'U') || (nextMode == 'L') || (nextMode == 'M') || (nextMode == 'P') || (nextMode == 'B')) && (count <= 9))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'U';
                        }
                    }

                    if (currentMode == 'L')
                    {
                        if ((nextMode == 'E') && (count <= 5))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'L';
                        }

                        if ((nextMode == 'U') && (count == 1))
                            reducedEncodeMode[i] = 'L';

                        if ((nextMode == 'L') && (count <= 14))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'L';
                        }

                        if (((nextMode == 'M') || (nextMode == 'P') || (nextMode == 'B')) && (count <= 9))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'L';
                        }
                    }

                    if (currentMode == 'M')
                    {
                        if (((nextMode == 'E') || (nextMode == 'U')) && (count <= 9))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'M';
                        }

                        if (((nextMode == 'L') || (nextMode == 'B')) && (count <= 14))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'M';
                        }

                        if (((nextMode == 'M') || (nextMode == 'P')) && (count <= 19))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'M';
                        }
                    }

                    if (currentMode == 'P')
                    {
                        if ((nextMode == 'E') && (count <= 5))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'U';
                        }

                        if (((nextMode == 'U') || (nextMode == 'L') || (nextMode == 'M') || (nextMode == 'P') || (nextMode == 'B')) && (count <= 9))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'U';
                        }
                    }

                    // Default is Digit mode
                    if (reducedEncodeMode[i] == 'X')
                        reducedEncodeMode[i] = 'D';
                }

                if (reducedEncodeMode[i] != 'B')
                    currentMode = reducedEncodeMode[i];
            }

            // Decide when to use P/S instead of P/L and U/S instead of U/L
            currentMode = 'U';
            for (int i = 0; i < reducedLength; i++)
            {
                if (reducedEncodeMode[i] != currentMode)
                {
                    for (count = 0; ((i + count) < reducedLength) && (reducedEncodeMode[i + count] == reducedEncodeMode[i]); count++) ;
                    nextMode = GetNextMode(reducedEncodeMode, reducedLength, i);
                    if (reducedEncodeMode[i] == 'P')
                    {
                        if ((currentMode == 'U') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'p';
                        }

                        if ((currentMode == 'L') && (nextMode != 'U') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'p';
                        }

                        if ((currentMode == 'L') && (nextMode == 'U') && (count == 1))
                            reducedEncodeMode[i] = 'p';

                        if ((currentMode == 'M') && (nextMode != 'M') && (count == 1))
                            reducedEncodeMode[i] = 'p';

                        if ((currentMode == 'M') && (nextMode == 'M') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'p';
                        }

                        if ((currentMode == 'D') && (nextMode != 'D') && (count <= 3))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'p';
                        }

                        if ((currentMode == 'D') && (nextMode == 'D') && (count <= 6))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'p';
                        }
                    }

                    if (reducedEncodeMode[i] == 'U')
                    {
                        if ((currentMode == 'L') && ((nextMode == 'L') || (nextMode == 'M')) && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'u';
                        }

                        if ((currentMode == 'L') && ((nextMode == 'E') || (nextMode == 'D') || (nextMode == 'B') || (nextMode == 'P')) && (count == 1))
                            reducedEncodeMode[i] = 'u';

                        if ((currentMode == 'D') && (nextMode == 'D') && (count == 1))
                            reducedEncodeMode[i] = 'u';

                        if ((currentMode == 'D') && (nextMode == 'P') && (count <= 2))
                        {
                            for (int j = 0; j < count; j++)
                                reducedEncodeMode[i + j] = 'u';
                        }
                    }
                }

                if ((reducedEncodeMode[i] != 'p') && (reducedEncodeMode[i] != 'u') && (reducedEncodeMode[i] != 'B'))
                    currentMode = reducedEncodeMode[i];
            }

            if (isGS1)
            {
                bitStream.AppendBits(0, 5); // P/S
                bitStream.AppendBits(0, 5); // FLG(n)
                bitStream.AppendBits(0, 3); // FLG(0)
            }

            if (eci != 3)
            {
                bitStream.AppendBits(0, 5); // P/S
                bitStream.AppendBits(0, 5); // FLG(n)
                if (eci < 10)
                {
                    bitStream.AppendBits(1, 3); // FLG(1)
                    bitStream.AppendBits(2 + eci, 4);
                }

                if ((eci >= 10) && (eci <= 99))
                {
                    bitStream.AppendBits(2, 3); // FLG(2)
                    bitStream.AppendBits(2 + (eci / 10), 4);
                    bitStream.AppendBits(2 + (eci % 10), 4);
                }

                if ((eci >= 100) && (eci <= 999))
                {
                    bitStream.AppendBits(3, 3); // FLG(3)
                    bitStream.AppendBits(2 + (eci / 100), 4);
                    bitStream.AppendBits(2 + ((eci % 100) / 10), 4);
                    bitStream.AppendBits(2 + (eci % 10), 4);
                }

                if ((eci >= 1000) && (eci <= 9999))
                {
                    bitStream.AppendBits(4, 3); // FLG(4)
                    bitStream.AppendBits(2 + (eci / 1000), 4);
                    bitStream.AppendBits(2 + ((eci % 1000) / 100), 4);
                    bitStream.AppendBits(2 + ((eci % 100) / 10), 4);
                    bitStream.AppendBits(2 + (eci % 10), 4);
                }

                if ((eci >= 10000) && (eci <= 99999))
                {
                    bitStream.AppendBits(5, 3); // FLG(5) 
                    bitStream.AppendBits(2 + (eci / 10000), 4);
                    bitStream.AppendBits(2 + ((eci % 10000) / 1000), 4);
                    bitStream.AppendBits(2 + ((eci % 1000) / 100), 4);
                    bitStream.AppendBits(2 + ((eci % 100) / 10), 4);
                    bitStream.AppendBits(2 + (eci % 10), 4);
                }

                if (eci >= 100000)
                {
                    bitStream.AppendBits(6, 3); // FLG(6)
                    bitStream.AppendBits(2 + (eci / 100000), 4);
                    bitStream.AppendBits(2 + ((eci % 100000) / 10000), 4);
                    bitStream.AppendBits(2 + ((eci % 10000) / 1000), 4);
                    bitStream.AppendBits(2 + ((eci % 1000) / 100), 4);
                    bitStream.AppendBits(2 + ((eci % 100) / 10), 4);
                    bitStream.AppendBits(2 + (eci % 10), 4);
                }
            }

            currentMode = 'U';
            for (int i = 0; i < reducedLength; i++)
            {
                if (reducedEncodeMode[i] != currentMode)
                {
                    // Change mode
                    if (currentMode == 'U')
                    {
                        switch (reducedEncodeMode[i])
                        {
                            case 'L':
                                bitStream.AppendBits(28, 5); // L/L
                                break;

                            case 'M':
                                bitStream.AppendBits(29, 5); // M/L
                                break;

                            case 'P':
                                bitStream.AppendBits(29, 5); // M/L
                                bitStream.AppendBits(30, 5); // P/L
                                break;

                            case 'p':
                                bitStream.AppendBits(0, 5); // P/S
                                break;

                            case 'D':
                                bitStream.AppendBits(30, 5); // D/L
                                break;

                            case 'B':
                                bitStream.AppendBits(31, 5); // B/S
                                break;
                        }
                    }

                    if (currentMode == 'L')
                    {
                        switch (reducedEncodeMode[i])
                        {
                            case 'U':
                                bitStream.AppendBits(30, 5); // D/L
                                bitStream.AppendBits(14, 4); // U/L
                                break;

                            case 'u':
                                bitStream.AppendBits(28, 5); // U/S
                                break;

                            case 'M':
                                bitStream.AppendBits(29, 5); // M/L
                                break;

                            case 'P':
                                bitStream.AppendBits(30, 5); // P/L
                                break;

                            case 'p':
                                bitStream.AppendBits(0, 5); // P/S
                                break;

                            case 'D':
                                bitStream.AppendBits(30, 5); // D/L
                                break;

                            case 'B':
                                bitStream.AppendBits(31, 5); // B/S
                                break;
                        }
                    }

                    if (currentMode == 'M')
                    {
                        switch (reducedEncodeMode[i])
                        {
                            case 'U':
                                bitStream.AppendBits(29, 5); // U/L
                                break;

                            case 'L':
                                bitStream.AppendBits(28, 5); // L/L
                                break;

                            case 'P':
                                bitStream.AppendBits(30, 5); // P/L
                                break;

                            case 'p':
                                bitStream.AppendBits(0, 5); // P/S
                                break;

                            case 'D':
                                bitStream.AppendBits(29, 5); // U/L
                                bitStream.AppendBits(30, 5); // D/L
                                break;

                            case 'B':
                                bitStream.AppendBits(31, 5); // B/S
                                break;
                        }
                    }

                    if (currentMode == 'P')
                    {
                        switch (reducedEncodeMode[i])
                        {
                            case 'U':
                                bitStream.AppendBits(31, 5); // U/L
                                break;

                            case 'L':
                                bitStream.AppendBits(31, 5); // U/L
                                bitStream.AppendBits(28, 5);
                                break;

                            case 'M':
                                bitStream.AppendBits(31, 5); // U/L
                                bitStream.AppendBits(29, 5); // M/L
                                break;

                            case 'D':
                                bitStream.AppendBits(31, 5); // U/L
                                bitStream.AppendBits(30, 5); // D/L
                                break;

                            case 'B':
                                bitStream.AppendBits(31, 5); // U/L
                                currentMode = 'U';
                                bitStream.AppendBits(31, 5); // B/S
                                break;
                        }
                    }

                    if (currentMode == 'D')
                    {
                        switch (reducedEncodeMode[i])
                        {
                            case 'U':
                                bitStream.AppendBits(14, 4); // U/L
                                break;

                            case 'u':
                                bitStream.AppendBits(15, 4); // U/S
                                break;

                            case 'L':
                                bitStream.AppendBits(14, 4); // U/L
                                bitStream.AppendBits(28, 5); // L/L
                                break;

                            case 'M':
                                bitStream.AppendBits(14, 4); // U/L
                                bitStream.AppendBits(29, 5); // M/L
                                break;

                            case 'P':
                                bitStream.AppendBits(14, 4); // U/L
                                bitStream.AppendBits(29, 5); // M/L
                                bitStream.AppendBits(30, 5); // P/L
                                break;

                            case 'p':
                                bitStream.AppendBits(0, 4); // P/S
                                break;

                            case 'B':
                                bitStream.AppendBits(14, 4); // U/L
                                currentMode = 'U';
                                bitStream.AppendBits(31, 5); // B/S
                                break;
                        }
                    }

                    // Byte mode length descriptor
                    if ((reducedEncodeMode[i] == 'B') && (!byteMode))
                    {
                        for (count = 0; ((i + count) < reducedLength) && (reducedEncodeMode[i] == 'B'); count++) ;

                        if (count > 2079)
                            return false;

                        if (count > 31) // Put 00000 followed by 11-bit number of bytes less 31.
                        {
                            bitStream.AppendBits(0, 5);
                            bitStream.AppendBits(count - 31, 11);
                        }

                        else // Put 5-bit number of bytes.
                            bitStream.AppendBits(count, 5);

                        byteMode = true;
                    }

                    if ((reducedEncodeMode[i] != 'B') && byteMode)
                        byteMode = false;

                    if ((reducedEncodeMode[i] != 'B') && (reducedEncodeMode[i] != 'u') && (reducedEncodeMode[i] != 'p'))
                        currentMode = reducedEncodeMode[i];
                }

                if ((reducedEncodeMode[i] == 'U') || (reducedEncodeMode[i] == 'u'))
                {
                    if (reducedSource[i] == ' ')
                        bitStream.AppendBits(1, 5);

                    else
                        bitStream.AppendBits(AztecSymbolCharacter[(int)reducedSource[i]], 5);
                }

                if (reducedEncodeMode[i] == 'L')
                {
                    if (reducedSource[i] == ' ')
                        bitStream.AppendBits(1, 5); // SP

                    else
                        bitStream.AppendBits(AztecSymbolCharacter[(int)reducedSource[i]], 5);
                }

                if (reducedEncodeMode[i] == 'M')
                {
                    if (reducedSource[i] == ' ')
                        bitStream.AppendBits(1, 5); // SP

                    else if (reducedSource[i] == 13)
                        bitStream.AppendBits(14, 5); // CR

                    else
                        bitStream.AppendBits(AztecSymbolCharacter[(int)reducedSource[i]], 5);
                }

                if ((reducedEncodeMode[i] == 'P') || (reducedEncodeMode[i] == 'p'))
                {
                    if (isGS1 && (reducedSource[i] == '['))
                        bitStream.AppendBits(0, 5); // FLG(0) = FNC1

                    else if (reducedSource[i] == 13)
                        bitStream.AppendBits(1, 5); // CR

                    else if (reducedSource[i] == 'a')
                        bitStream.AppendBits(2, 5); // CR LF

                    else if (reducedSource[i] == 'b')
                        bitStream.AppendBits(3, 5);// . SP

                    else if (reducedSource[i] == 'c')
                        bitStream.AppendBits(4, 5); // , SP

                    else if (reducedSource[i] == 'd')
                        bitStream.AppendBits(5, 5); // : SP

                    else if (reducedSource[i] == ',')
                        bitStream.AppendBits(17, 5); // Comma

                    else if (reducedSource[i] == '.')
                        bitStream.AppendBits(19, 5); // Full stop

                    else
                        bitStream.AppendBits(AztecSymbolCharacter[(int)reducedSource[i]], 5);
                }

                if (reducedEncodeMode[i] == 'D')
                {
                    if (reducedSource[i] == ' ')
                        bitStream.AppendBits(1, 4); // SP

                    else if (reducedSource[i] == ',')
                        bitStream.AppendBits(12, 4); // Comma

                    else if (reducedSource[i] == '.')
                        bitStream.AppendBits(13, 4); // Full stop

                    else
                        bitStream.AppendBits(AztecSymbolCharacter[(int)reducedSource[i]], 4);
                }

                if (reducedEncodeMode[i] == 'B')
                    bitStream.AppendBits(reducedSource[i], 8);
            }

            return true;
        }

        // Calculate the position of the bits in the grid.
        private static void PopulateMap()
        {
            int layer, start, length, n, i;
            int x, y;
            /*for (x = 0; x < 151; x++)
            {
                for (y = 0; y < 151; y++)
                    AztecMap[(x * 151) + y] = 0;
            }*/

            for (layer = 1; layer < 33; layer++)
            {
                start = (112 * (layer - 1)) + (16 * (layer - 1) * (layer - 1)) + 2;
                length = 28 + ((layer - 1) * 4) + (layer * 4);

                // Top.
                i = 0;
                x = 64 - ((layer - 1) * 2);
                y = 63 - ((layer - 1) * 2);
                for (n = start; n < (start + length); n += 2)
                {
                    AztecMap[(AvoidReferenceGrid(y) * 151) + AvoidReferenceGrid(x + i)] = n;
                    AztecMap[(AvoidReferenceGrid(y - 1) * 151) + AvoidReferenceGrid(x + i)] = n + 1;
                    i++;
                }

                // Right.
                i = 0;
                x = 78 + ((layer - 1) * 2);
                y = 64 - ((layer - 1) * 2);
                for (n = start + length; n < (start + (length * 2)); n += 2)
                {
                    AztecMap[(AvoidReferenceGrid(y + i ) * 151) + AvoidReferenceGrid(x)] = n;
                    AztecMap[(AvoidReferenceGrid(y + i) * 151) + AvoidReferenceGrid(x + 1)] = n + 1;
                    i++;
                }

                // Bottom.
                i = 0;
                x = 77 + ((layer - 1) * 2);
                y = 78 + ((layer - 1) * 2);
                for (n = start + (length * 2); n < (start + (length * 3)); n += 2)
                {
                    AztecMap[(AvoidReferenceGrid(y) * 151) + AvoidReferenceGrid(x - i)] = n;
                    AztecMap[(AvoidReferenceGrid(y + 1) * 151) + AvoidReferenceGrid(x - i)] = n + 1;
                    i++;
                }

                // Left.
                i = 0;
                x = 63 - ((layer - 1) * 2);
                y = 77 + ((layer - 1) * 2);
                for (n = start + (length * 3); n < (start + (length * 4)); n += 2)
                {
                    AztecMap[(AvoidReferenceGrid(y - i) * 151) + AvoidReferenceGrid(x)] = n;
                    AztecMap[(AvoidReferenceGrid(y - i) * 151) + AvoidReferenceGrid(x - 1)] = n + 1;
                    i++;
                }
            }

            // Central finder pattern.
            for (y = 69; y <= 81; y++)
            {
                for (x = 69; x <= 81; x++)
                    AztecMap[(x * 151) + y] = 1;
            }

            for (y = 70; y <= 80; y++)
            {
                for (x = 70; x <= 80; x++)
                    AztecMap[(x * 151) + y] = 0;
            }

            for (y = 71; y <= 79; y++)
            {
                for (x = 71; x <= 79; x++)
                    AztecMap[(x * 151) + y] = 1;
            }

            for (y = 72; y <= 78; y++)
            {
                for (x = 72; x <= 78; x++)
                    AztecMap[(x * 151) + y] = 0;
            }

            for (y = 73; y <= 77; y++)
            {
                for (x = 73; x <= 77; x++)
                    AztecMap[(x * 151) + y] = 1;
            }

            for (y = 74; y <= 76; y++)
            {
                for (x = 74; x <= 76; x++)
                    AztecMap[(x * 151) + y] = 0;
            }

            // Guide bars.
            for (y = 11; y < 151; y += 16)
            {
                for (x = 1; x < 151; x += 2)
                {
                    AztecMap[(x * 151) + y] = 1;
                    AztecMap[(y * 151) + x] = 1;
                }
            }

            // Descriptor.
            for (i = 0; i < 10; i++)
                AztecMap[(AvoidReferenceGrid(64) * 151) + AvoidReferenceGrid(66 + i)] = 20000 + i;  // Top.

            for (i = 0; i < 10; i++)
                AztecMap[(AvoidReferenceGrid(66 + i) * 151) + AvoidReferenceGrid(77)] = 20010 + i;  // Right.

            for (i = 0; i < 10; i++)
                AztecMap[(AvoidReferenceGrid(77) * 151) + AvoidReferenceGrid(75 - i)] = 20020 + i;  // Bottom.

            for (i = 0; i < 10; i++)
                AztecMap[(AvoidReferenceGrid(75 - i) * 151) + AvoidReferenceGrid(64)] = 20030 + i;  // Left.

            // Orientation.
            AztecMap[(AvoidReferenceGrid(64) * 151) + AvoidReferenceGrid(64)] = 1;
            AztecMap[(AvoidReferenceGrid(65) * 151) + AvoidReferenceGrid(64)] = 1;
            AztecMap[(AvoidReferenceGrid(64) * 151) + AvoidReferenceGrid(65)] = 1;
            AztecMap[(AvoidReferenceGrid(64) * 151) + AvoidReferenceGrid(77)] = 1;
            AztecMap[(AvoidReferenceGrid(65) * 151) + AvoidReferenceGrid(77)] = 1;
            AztecMap[(AvoidReferenceGrid(76) * 151) + AvoidReferenceGrid(77)] = 1;
        }

        // Prevent data from obscuring reference grid.
        private static int AvoidReferenceGrid(int output)
        {
            if (output > 10)
                output++;

            if (output > 26)
                output++;

            if (output > 42)
                output++;

            if (output > 58)
                output++;

            if (output > 74)
                output++;

            if (output > 90)
                output++;

            if (output > 106)
                output++;

            if (output > 122)
                output++;

            if (output > 138)
                output++;

            return output;
        }

        private int CountDoubles(int position, int inputLength)
        {
            int c = 0;
            int i = position;
            bool condition = true;

            do
            {
                if (((barcodeData[i] == '.') || (barcodeData[i] == ',')) && (barcodeData[i + 1] == ' '))
                    c++;

                else
                    condition = false;

                i += 2;
            } while ((i < inputLength) && condition);

            return c;
        }

        private static int CountCR(char[] source, int position, int length)
        {
            int c = 0;
            int i = position;
            bool condition = true;

            do
            {
                if (source[i] == 13)
                    c++;

                else
                    condition = false;

                i++;
            } while ((i < length) && condition);

            return c;
        }

        private static int CountDotComma(char[] source, int position, int length)
        {
            int c = 0;
            int i = position;
            bool condition = true;

            do
            {
                if ((source[i] == '.') || (source[i] == ','))
                    c++;

                else
                    condition = false;

                i++;
            } while ((i < length) && condition);

            return c;
        }

        private static int CountSpaces(char[] source, int position, int length)
        {
            int c = 0;
            int i = position;
            bool condition = true;

            do
            {
                if (source[i] == ' ')
                    c++;

                else
                    condition = false;

                i++;
            } while ((i < length) && condition);

            return c;
        }

        private static char GetNextMode(char[] encodeMode, int inputLength, int position)
        {
            int i = position;

            do
            {
                i++;
            } while ((i < inputLength) && (encodeMode[i] == encodeMode[position]));

            if (i >= inputLength)
                return 'E';

            else
                return encodeMode[i];
        }

        private void BuildSymbolData(BitVector binaryPattern, int layers, bool isCompactSymbol)
        {
            // Expand the row pattern into the symbol data.
            byte[] rowData;
            if (isCompactSymbol)
            {
                int offset = AztecCompactOffset[layers - 1];
                int size = 27 - (2 * offset);

                for (int y = offset; y < (27 - offset); y++)
                {
                    rowData = new byte[size];
                    for (int x = offset; x < (27 - offset); x++)
                    {
                        if (CompactAztecMap[(y * 27) + x] == 1)
                            rowData[x - offset] = 1;

                        if (CompactAztecMap[(y * 27) + x] >= 2)
                        {
                            if (binaryPattern[CompactAztecMap[(y * 27) + x] - 2] == 1)
                                rowData[x - offset] = 1;
                        }
                    }

                    SymbolData symbolData = new SymbolData(rowData, 1.0f);
                    Symbol.Insert(y - offset, symbolData);
                }
            }

            else
            {
                int offset = AztecOffset[layers - 1];
                int size = 151 - (2 * offset);

                for (int y = offset; y < (151 - offset); y++)
                {
                    rowData = new byte[size];
                    for (int x = offset; x < (151 - offset); x++)
                    {
                        if (AztecMap[(y * 151) + x] == 1)
                            rowData[x - offset] = 1;

                        if (AztecMap[(y * 151) + x] >= 2)
                        {
                            if (binaryPattern[AztecMap[(y * 151) + x] - 2] == 1)
                                rowData[x - offset] = 1;
                        }
                    }

                    SymbolData symbolData = new SymbolData(rowData, 1.0f);
                    Symbol.Insert(y - offset, symbolData);
                }
            }
        }

        /// <summary>
        /// Generate Aztec Runes symbol.
        /// </summary>
        private void AztecRunes()
        {
            int inputValue;
            int inputLength = barcodeData.Length;
            BitVector bitStream = new BitVector();
            byte[] dataCodewords = new byte[2];
            byte[] eccCodewords = new byte[5];

            if (inputLength > 3)
                throw new InvalidDataLengthException("Aztec Runes: Maximum input of 3 characters.");

            inputValue = int.Parse(barcodeMessage, CultureInfo.CurrentCulture);
            if (inputValue > 255)
                throw new InvalidDataException("Aztec Runes: Maximum numeric value 255.");

            bitStream.AppendBits(inputValue, 8);
            
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (bitStream[(i * 4) + j] == 1)
                        dataCodewords[i] += (byte)(0x08 >> j);
                }
            }

            ReedSolomon.RSInitialise(0x13, 5, 1);
            ReedSolomon.RSEncode(2, dataCodewords, eccCodewords);
            bitStream.Clear();
            bitStream.AppendBits(0, 8);

            for (int i = 0; i < 5; i++)
                bitStream.AppendBits(eccCodewords[4 - i], 4);

            int length = bitStream.SizeInBits;
            for (int i = 0; i < length; i += 2)
            {
                if (bitStream[i] == 0)
                    bitStream[i] = 1;

                else
                    bitStream[i] = 0;
            }

            // Expand the row pattern into the symbol data.
            byte[] rowData;
            int size = 11;

            for (int y = 8; y < 19; y++)
            {
                rowData = new byte[size];
                for (int x = 8; x < 19; x++)
                {
                    if (CompactAztecMap[(y * 27) + x] == 1)
                        rowData[x - 8] = 1;

                    if (CompactAztecMap[(y * 27) + x] >= 2)
                    {
                        if(bitStream[CompactAztecMap[(y * 27) + x] - 2000] == 1)
                            rowData[x - 8] = 1;
                    }
                }

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                int row = y - 8;
                Symbol.Insert(row, symbolData);
            }
        }
    }
}
