/* DataMatrixEncoder.cs Handles Data Matrix ECC 200 2D symbol */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Robin Stuart and other Zint Authors and Contributors.
  
    libzint - the open source barcode library
    Copyright (C) 2009-2016 Robin Stuart <rstuart114@gmail.com>

    developed from and including some functions from:
        IEC16022 bar code generation
        Adrian Kennard, Andrews & Arnold Ltd
        with help from Cliff Hones on the RS coding

        (c) 2004 Adrian Kennard, Andrews & Arnold Ltd
        (c) 2006 Stefan Schmidt <stefan@datenfreihafen.org>

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
    /// <summary>
    /// Builds a DataMatrix Symbol
    /// </summary>
    internal class DataMatrixEncoder : SymbolEncoder
    {
        # region Tables and Constants
        private static int[] C40Shift = {
	        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
	        0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	        2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

        private static int[] C40Values = {
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 4, 5, 6,
            7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 19, 20, 21, 14, 15, 16, 17, 18, 19, 20, 21, 22,
            23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 22, 23, 24, 25, 26,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31 };

        private static int[] TextShift = {
	        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            2, 2, 2, 2, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3 };

        private static int[] TextValues = {
	        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
            15, 16, 17, 18, 19, 20, 21, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26,
            22, 23, 24, 25, 26, 0, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 27, 28, 29, 30, 31 };

        private static int[] IntSymbols = {
			 0, /*  1: 10x10 ,  3*/  1, /*  2: 12x12 ,  5*/  3, /*  3: 14x14 ,  8*/  5, /*  4: 16x16 , 12*/
             7, /*  5: 18x18 , 18*/  9, /*  6: 20x20 , 22*/ 12, /*  7: 22x22 , 30*/ 14, /*  8: 24x24 , 36*/
            16, /*  9: 26x26 , 44*/ 18, /* 10: 32x32 , 62*/ 22, /* 11: 36x36 , 86*/ 25, /* 12: 40x40 ,114*/
            27, /* 13: 44x44 ,144*/ 28, /* 14: 48x48 ,174*/ 29, /* 15: 52x52 ,204*/ 30, /* 16: 64x64 ,280*/
            31, /* 17: 72x72 ,368*/ 32, /* 18: 80x80 ,456*/ 33, /* 19: 88x88 ,576*/ 34, /* 20: 96x96 ,696*/
            35, /* 21:104x104,816*/ 36, /* 22:120x120,1050*/37, /* 23:132x132,1304*/38, /* 24:144x144,1558*/
             2, /* 25:  8x18 ,  5*/  4, /* 26:  8x32 , 10*/  6, /* 27: 12x26 , 16*/ 10, /* 28: 12x36 , 22*/
            13, /* 29: 16x36 , 32*/ 17, /* 30: 16x48 , 49*/  8, /* 31:  8x48 , 18*/ 11, /* 32:  8x64 , 24*/
            15, /* 33: 12x64 , 43*/ 19, /* 34: 16x64 , 62*/ 21, /* 35: 24x48 , 80*/ 24, /* 36: 24x64 ,108*/
            20, /* 37: 26x40 , 70*/ 23, /* 38: 26x48 , 90*/ 26, /* 39: 26x64 ,118*/ 0 };

        private static bool[] IsDMRE = {
            /* 0*/ false, /*  10x10, 3 */ false, /* 12x12 , 5 */ false, /*  8x18 , 5 */ false, /* 14x14 , 8 */
            /* 4*/ false, /*  8x32 ,10 */ false, /* 16x16 ,12 */ false, /* 12x26 ,16 */ false, /* 18x18 ,18 */
            /* 8*/ true, /*  8x48 ,18 */ false, /* 20x20 ,22 */ false, /* 12x36 ,22 */ true, /*  8x64 ,24 */
            /*12*/ false, /* 22x22 ,30 */ false, /* 16x36 ,32 */ false, /* 24x24 ,36 */ true, /* 12x64 ,43 */
            /*16*/ false, /* 26x26 ,44 */ false, /* 16x48 ,49 */ false, /* 32x32 ,62 */ true, /* 16x64 ,62 */
            /*20*/ true, /* 26x40 , 70*/ true, /* 24x48 ,80 */ false, /* 36x36 ,86 */ true, /* 26x48 ,90 */
            /*24*/ true, /* 24x64 ,108*/ false, /* 40x40 ,114*/ true, /* 26x64 ,118*/ false, /* 44x44 ,144*/
            /*28*/ false, /* 48x48,174 */ false, /* 52x52,204 */ false, /* 64x64,280 */ false, /* 72x72,368 */
            /*32*/ false, /* 80x80,456 */ false, /* 88x88,576 */ false, /* 96x96,696 */ false, /*104x104,816*/
            /*36*/ false, /*120x120,1050*/ false, /*132x132,1304*/false  /*144x144,1558*/ };

        private static int[] MatrixHeights = {
	        /* 0*/ 10, /* 10x10 , 3 */ 12, /* 12x12 , 5 */ 8, /*   8x18 , 5 */ 14, /* 14x14 , 8 */
            /* 4*/  8, /*  8x32 ,10 */ 16, /* 16x16 ,12 */ 12, /* 12x26 ,16 */ 18, /* 18x18 ,18 */
            /* 8*/  8, /*  8x48 ,18 */ 20, /* 20x20 ,22 */ 12, /* 12x36 ,22 */ 8, /*   8x64 ,24 */
            /*12*/ 22, /* 22x22 ,30 */ 16, /* 16x36 ,32 */ 24, /* 24x24 ,36 */ 12, /* 12x64 ,43 */
            /*16*/ 26, /* 26x26 ,44 */ 16, /* 16x48 ,49 */ 32, /* 32x32 ,62 */ 16, /* 16x64 ,62 */
            /*20*/ 26, /* 26x40 , 70*/ 24, /* 24x48 ,80 */ 36, /* 36x36 ,86 */ 26, /* 26x48 ,90 */
            /*24*/ 24, /* 24x64 ,108*/ 40, /* 40x40 ,114*/ 26, /* 26x64 ,118*/ 44, /* 44x44 ,144*/
            /*28*/ 48, /* 48x48,174 */ 52, /* 52x52,204 */ 64, /* 64x64,280 */ 72, /* 72x72,368 */
            /*32*/ 80, /* 80x80,456 */ 88, /* 88x88,576 */ 96, /* 96x96,696 */ 104, /*104x104,816*/
            /*36*/ 120, /*120x120,1050*/ 132,/*132x132,1304*/144 /*144x144,1558*/ };

        private static int[] MatrixWidths = {
	        /* 0*/ 10, /* 10x10 */ 12, /* 12x12 */ 18, /*  8x18 */ 14, /* 14x14 */
            /* 4*/ 32, /*  8x32 */ 16, /* 16x16 */ 26, /* 12x26 */ 18, /* 18x18 */
            /* 8*/ 48, /*  8x48 */ 20, /* 20x20 */ 36, /* 12x36 */ 64, /*  8x64 */
            /*12*/ 22, /* 22x22 */ 36, /* 16x36 */ 24, /* 24x24 */ 64, /* 12x64 */
            /*16*/ 26, /* 26x26 */ 48, /* 16x48 */ 32, /* 32x32 */ 64, /* 16x64 */
            /*20*/ 40, /* 26x40 */ 48, /* 24x48 */ 36, /* 36x36 */ 48, /* 26x48 */
            /*24*/ 64, /* 24x64 */ 40, /* 40x40 */ 64, /* 26x64 */ 44, /* 44x44 */
            /*28*/ 48, /* 48x48 */ 52, /* 52x52 */ 64, /* 64x64 */ 72, /* 72x72 */
            /*32*/ 80, /* 80x80 */ 88, /* 88x88 */ 96, /* 96x96 */ 104,/*104x104*/
            /*36*/ 120,/*120x120*/132, /*132x132*/144  /*144x144*/ };

        // Horizontal submodule size (including subfinder)
        private static int[] MatrixFH = {
	        /* 0*/ 10, /* 10x10 */ 12, /* 12x12 */ 8, /*   8x18 */ 14, /* 14x14 */
            /* 4*/  8, /*  8x32 */ 16, /* 16x16 */ 12, /* 12x26 */ 18, /* 18x18 */
            /* 8*/  8, /*  8x48 */ 20, /* 20x20 */ 12, /* 12x36 */  8, /*  8x64 */
            /*12*/ 22, /* 22x22 */ 16, /* 16x36 */ 24, /* 24x24 */ 12, /* 12x64 */
            /*16*/ 26, /* 26x26 */ 16, /* 16x48 */ 16, /* 32x32 */ 16, /* 16x64 */
            /*20*/ 26, /* 26x40 */ 24, /* 24x48 */ 18, /* 36x36 */ 26, /* 26x48 */
            /*24*/ 24, /* 24x64 */ 20, /* 40x40 */ 26, /* 26x64 */ 22, /* 44x44 */
            /*28*/ 24, /* 48x48 */ 26, /* 52x52 */ 16, /* 64x64 */ 18, /* 72x72 */
            /*32*/ 20, /* 80x80 */ 22, /* 88x88 */ 24, /* 96x96 */ 26, /*104x104*/
            /*36*/ 20, /*120x120*/ 22, /*132x132*/ 24  /*144x144*/ };

        // Vertical submodule size (including subfinder)
        private static int[] MatrixFW = {
	        /* 0*/ 10, /* 10x10 */ 12, /* 12x12 */ 18, /*  8x18 */ 14, /* 14x14 */
            /* 4*/ 16, /*  8x32 */ 16, /* 16x16 */ 26, /* 12x26 */ 18, /* 18x18 */
            /* 8*/ 24, /*  8x48 */ 20, /* 20x20 */ 18, /* 12x36 */ 16, /*  8x64 */
            /*12*/ 22, /* 22x22 */ 18, /* 16x36 */ 24, /* 24x24 */ 16, /* 12x64 */
            /*16*/ 26, /* 26x26 */ 24, /* 16x48 */ 16, /* 32x32 */ 16, /* 16x64 */
            /*20*/ 20, /* 26x40 */ 24, /* 24x48 */ 18, /* 36x36 */ 24, /* 26x48 */
            /*24*/ 16, /* 24x64 */ 20, /* 40x40 */ 16, /* 26x64 */ 22, /* 44x44 */
            /*28*/ 24, /* 48x48 */ 26, /* 52x52 */ 16, /* 64x64 */ 18, /* 72x72 */
            /*32*/ 20, /* 80x80 */ 22, /* 88x88 */ 24, /* 96x96 */ 26, /*104x104*/
            /*36*/ 20, /*120x120*/ 22, /*132x132*/ 24  /*144x144*/ };

        private static int[] MatrixBytes = {
	        /* 0*/   3, /* 10x10 */   5, /* 12x12 */   5, /*  8x18 */   8, /* 14x14 */
            /* 4*/  10, /*  8x32 */  12, /* 16x16 */  16, /* 12x26 */  18, /* 18x18 */
            /* 8*/  18, /*  8x48 */  22, /* 20x20 */  22, /* 12x36 */  24, /*  8x64 */
            /*12*/  30, /* 22x22 */  32, /* 16x36 */  36, /* 24x24 */  43, /* 12x64 */
            /*16*/  44, /* 26x26 */  49, /* 16x48 */  62, /* 32x32 */  62, /* 16x64 */
            /*20*/  70, /* 26x40 */  80, /* 24x48 */  86, /* 36x36 */  90, /* 26x48 */
            /*24*/ 108, /* 24x64 */ 114, /* 40x40 */ 118, /* 26x64 */ 144, /* 44x44 */
            /*28*/ 174, /* 48x48 */ 204, /* 52x52 */ 280, /* 64x64 */ 368, /* 72x72 */
            /*32*/ 456, /* 80x80 */ 576, /* 88x88 */ 696, /* 96x96 */ 816, /*104x104*/
            /*36*/1050, /*120x120*/1304, /*132x132*/1558 /*144x144*/ };

        private static int[] MatrixDataBlocks = {
	        /* 0*/   3, /* 10x10 */   5, /* 12x12 */   5, /*  8x18 */   8, /* 14x14 */
            /* 4*/  10, /*  8x32 */  12, /* 16x16 */  16, /* 12x26 */  18, /* 18x18 */
            /* 8*/  18, /*  8x48 */  22, /* 20x20 */  22, /* 12x36 */  24, /*  8x64 */
            /*12*/  30, /* 22x22 */  32, /* 16x36 */  36, /* 24x24 */  43, /* 12x64 */
            /*16*/  44, /* 26x26 */  49, /* 16x48 */  62, /* 32x32 */  62, /* 16x64 */
            /*20*/  70, /* 26x40 */  80, /* 24x48 */  86, /* 36x36 */  90, /* 26x48 */
            /*24*/ 108, /* 24x64 */ 114, /* 40x40 */ 118, /* 26x64 */ 144, /* 44x44 */
            /*28*/ 174, /* 48x48 */ 102, /* 52x52 */ 140, /* 64x64 */  92, /* 72x72 */
            /*32*/ 114, /* 80x80 */ 144, /* 88x88 */ 174, /* 96x96 */ 136, /*104x104*/
            /*36*/ 175, /*120x120*/ 163, /*132x132*/ 156 /* 144x144*/ };

        private static int[] MatrixEccBlocks = {
	        /* 0*/  5, /* 10x10 */  7, /* 12x12 */  7, /*  8x18 */ 10, /* 14x14 */
            /* 4*/ 11, /*  8x32 */ 12, /* 16x16 */ 14, /* 12x26 */ 14, /* 18x18 */
            /* 8*/ 15, /*  8x48 */ 18, /* 20x20 */ 18, /* 12x36 */ 18, /*  8x64 */
            /*12*/ 20, /* 22x22 */ 24, /* 16x36 */ 24, /* 24x24 */ 27, /* 12x64 */
            /*16*/ 28, /* 26x26 */ 28, /* 16x48 */ 36, /* 32x32 */ 36, /* 16x64 */
            /*20*/ 38, /* 26x40 */ 41, /* 24x48 */ 42, /* 36x36 */ 42, /* 26x48 */
            /*24*/ 46, /* 24x64 */ 48, /* 40x40 */ 50, /* 26x64 */ 56, /* 44x44 */
            /*28*/ 68, /* 48x48 */ 42, /* 52x52 */ 56, /* 64x64 */ 36, /* 72x72 */
            /*32*/ 48, /* 80x80 */ 56, /* 88x88 */ 68, /* 96x96 */ 56, /*104x104*/
            /*36*/ 68, /*120x120*/ 62, /*132x132*/ 62 /*144x144*/ };

        // Number of DM Sizes
        private const int DMSIZESCOUNT = 39;    // Number of DM Sizes.
        private const int SYMBOL144 = 37;       // Number of 144x144 for special interlace.

        private const int ASCII = 1;
        private const int C40 = 2;
        private const int TEXT = 3;
        private const int X12 = 4;
        private const int EDIFACT = 5;
        private const int BASE256 = 6;
        # endregion

        private int optionSymbolSize;
        private bool optionSquareOnly;      // True if force a square symbol in autosize.
        private bool optionDMRE;            // True if using rectangular extensions.
        //private bool optionReader;          // True if reader initialization mode.

        public DataMatrixEncoder(Symbology symbology, string barcodeMessage, DataMatrixSize optionSymbolSize,
            bool optionSquareOnly, bool optionDMRE, int eci, EncodingMode mode)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.optionSymbolSize = (int)optionSymbolSize;
            this.optionSquareOnly = optionSquareOnly;
            this.optionDMRE = optionDMRE;
            this.eci = eci;
            this.encodingMode = mode;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
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

                case EncodingMode.HIBC:
                    isGS1 = false;
                    barcodeData = MessagePreProcessor.HIBCParser(barcodeMessage);
                    break;
            }

            DataMatrix();
            return Symbol;
        }

        private void DataMatrix()
        {
            byte[] symbolGrid;
            List<byte> dataStream = new List<byte>();
            bool skew = false;
            int[] processBuffer = new int[8];
            int processBytesLeft = 0;
            int optionSize = 0;
            int symbolSize = 0;
            int calculatedSize = DMSIZESCOUNT - 1;
            int lastMode = 0;
            int padLength = 0;

            int inputLength = barcodeData.Length;
            if (!ProcessInputData(dataStream, ref lastMode, processBuffer, ref processBytesLeft))
                throw new InvalidDataLengthException("Data Matrix: Input data is too long to fit into the symbol.");


            if (optionSymbolSize >= 1 && (int)optionSymbolSize <= DMSIZESCOUNT)
                optionSize = IntSymbols[optionSymbolSize - 1];

            else
                optionSize = -1;  // Auto sizing.

            for (int i = DMSIZESCOUNT - 1; i > -1; i--)
            {
                if (MatrixBytes[i] >= dataStream.Count + processBytesLeft) // Make allowance for any remaining data.
                    calculatedSize = i;
            }

            if (optionSquareOnly)
            {
                while (MatrixHeights[calculatedSize] != MatrixWidths[calculatedSize])
                    calculatedSize++;
            }

            else if (!optionDMRE)
            {
                while (IsDMRE[calculatedSize])
                    calculatedSize++;
            }

            symbolSize = optionSize;
            if (calculatedSize > optionSize)
            {
                symbolSize = calculatedSize;
                if (optionSize != -1)
                    throw new InvalidDataLengthException("Data Matrix: Input data to long for selected symbol size.");
            }

            // Process the remaining data.
            int symbolCharactersLeft = MatrixBytes[symbolSize] - dataStream.Count;
            ProcessRemainingData(dataStream, lastMode, symbolCharactersLeft, processBuffer, processBytesLeft, inputLength);

            padLength = MatrixBytes[symbolSize] - dataStream.Count;
            if (padLength > 0)
                AddPadding(dataStream, padLength);

            skew = (symbolSize == SYMBOL144) ? true : false;

            // Add error correction.
            int dataBlocks = MatrixDataBlocks[symbolSize];
            int eccBlocks = MatrixEccBlocks[symbolSize];
            AddErrorCorrection(dataStream, dataBlocks, eccBlocks, skew);

            // Placement.
            int matrixHeight = MatrixHeights[symbolSize];
            int matrixWidth = MatrixWidths[symbolSize];
            int fHeight = MatrixFH[symbolSize];
            int fWidth = MatrixFW[symbolSize];

            int numberOfColumns = matrixWidth - 2 * (matrixWidth / fWidth);
            int numberOfRows = matrixHeight - 2 * (matrixHeight / fHeight);
            int[] matrixLocations = new int[numberOfColumns * numberOfRows];
            Placement(matrixLocations, numberOfRows, numberOfColumns);

            symbolGrid = new byte[matrixWidth * matrixHeight];
            for (int y = 0; y < matrixHeight; y += fHeight)
            {
                for (int x = 0; x < matrixWidth; x++)
                    symbolGrid[y * matrixWidth + x] = 1;

                for (int x = 0; x < matrixWidth; x += 2)
                    symbolGrid[(y + fHeight - 1) * matrixWidth + x] = 1;
            }

            for (int x = 0; x < matrixWidth; x += fWidth)
            {
                for (int y = 0; y < matrixHeight; y++)
                    symbolGrid[y * matrixWidth + x] = 1;

                for (int y = 0; y < matrixHeight; y += 2)
                    symbolGrid[y * matrixWidth + x + fWidth - 1] = 1;
            }

            for (int y = 0; y < numberOfRows; y++)
            {
                for (int x = 0; x < numberOfColumns; x++)
                {
                    int binaryValue = 0;
                    int locationValue = matrixLocations[(numberOfRows - y - 1) * numberOfColumns + x];

                    if (locationValue == 1 || locationValue > 7)
                    {
                        if (locationValue > 7)
                            binaryValue = dataStream[(locationValue >> 3) - 1] & (1 << (locationValue & 7));

                        if (locationValue == 1 || binaryValue != 0)
                        {
                            int position = (1 + y + 2 * (y / (fHeight - 2))) * matrixWidth + 1 + x + 2 * (x / (fWidth - 2));
                            symbolGrid[position] = 1;
                        }
                    }
                }
            }

            byte[] rowData;
            for (int y = matrixHeight - 1; y >= 0; y--)
            {
                rowData = new byte[matrixWidth];
                for (int x = 0; x < matrixWidth; x++)
                    rowData[x] = symbolGrid[(matrixWidth * y) + x];

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Add(symbolData);
            }

        }

        // Annex M placement alorithm low level.
        static void PlacementBit(int[] matrixLocations, int numberOfRows, int numberOfColumns, int row, int column, int position, byte offset)
        {
            if (row < 0)
            {
                row += numberOfRows;
                column += 4 - ((numberOfRows + 4) % 8);
            }

            if (column < 0)
            {
                column += numberOfColumns;
                row += 4 - ((numberOfColumns + 4) % 8);
            }

            // Necessary for 26x32,26x40,26x48,36x120,36x144,72x120,72x144.
            if(row >= numberOfRows)
                row -= numberOfRows;

            // Check index limits
            //assert(r < NR);
            //assert(c < NC);
            // Check double-assignment
            //assert(0 == array[r * NC + c]);

            matrixLocations[row * numberOfColumns + column] = (position << 3) + offset;
        }

        static void PlacementBlock(int[] matrixLocations, int numberOfRows, int numberOfColumns, int row, int column, int position)
        {
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 2, column - 2, position, 7);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 2, column - 1, position, 6);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 1, column - 2, position, 5);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 1, column - 1, position, 4);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 1, column - 0, position, 3);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 0, column - 2, position, 2);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 0, column - 1, position, 1);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, row - 0, column - 0, position, 0);
        }

        static void PlacementCornerA(int[] matrixLocations, int numberOfRows, int numberOfColumns, int position)
        {
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 0, position, 7);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 1, position, 6);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 2, position, 5);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 2, position, 4);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 1, position, 3);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 1, position, 2);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 2, numberOfColumns - 1, position, 1);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 3, numberOfColumns - 1, position, 0);
        }

        static void PlacementCornerB(int[] matrixLocations, int numberOfRows, int numberOfColumns, int position)
        {
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 3, 0, position, 7);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 2, 0, position, 6);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 0, position, 5);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 4, position, 4);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 3, position, 3);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 2, position, 2);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 1, position, 1);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 1, position, 0);
        }

        static void PlacementCornerC(int[] matrixLocations, int numberOfRows, int numberOfColumns, int position)
        {
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 3, 0, position, 7);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 2, 0, position, 6);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 0, position, 5);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 2, position, 4);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 1, position, 3);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 1, position, 2);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 2, numberOfColumns - 1, position, 1);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 3, numberOfColumns - 1, position, 0);
        }

        static void PlacementCornerD(int[] matrixLocations, int numberOfRows, int numberOfColumns, int position)
        {
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, 0, position, 7);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, numberOfRows - 1, numberOfColumns - 1, position, 6);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 3, position, 5);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 2, position, 4);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 0, numberOfColumns - 1, position, 3);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 3, position, 2);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 2, position, 1);
            PlacementBit(matrixLocations, numberOfRows, numberOfColumns, 1, numberOfColumns - 1, position, 0);
        }

        // Annex M placement alorithm main function
        static void Placement(int[] matrixLocations, int numberOfRows, int numberOfColumns)
        {
            // Start.
            int position = 1;
            int row = 4;
            int column = 0;

            do
            {
                // Check corner
                if (row == numberOfRows && column == 0)
                    PlacementCornerA(matrixLocations, numberOfRows, numberOfColumns, position++);

                if (row == numberOfRows - 2 && column == 0 && numberOfColumns % 4 != 0)
                    PlacementCornerB(matrixLocations, numberOfRows, numberOfColumns, position++);

                if (row == numberOfRows - 2 && column == 0 && (numberOfColumns % 8) == 4)
                    PlacementCornerC(matrixLocations, numberOfRows, numberOfColumns, position++);

                if (row == numberOfRows + 4 && column == 2 && (numberOfColumns % 8) == 0)
                    PlacementCornerD(matrixLocations, numberOfRows, numberOfColumns, position++);

                // Up/Right
                do
                {
                    if (row < numberOfRows && column >= 0 && matrixLocations[row * numberOfColumns + column] == 0)
                        PlacementBlock(matrixLocations, numberOfRows, numberOfColumns, row, column, position++);

                    row -= 2;
                    column += 2;
                }
                while (row >= 0 && column < numberOfColumns);

                row++;
                column += 3;

                // Down/Left
                do
                {
                    if (row >= 0 && column < numberOfColumns && matrixLocations[row * numberOfColumns + column] == 0)
                        PlacementBlock(matrixLocations, numberOfRows, numberOfColumns, row, column, position++);

                    row += 2;
                    column -= 2;
                }
                while (row < numberOfRows && column >= 0);

                row += 3;
                column++;
            }
            while (row < numberOfRows || column < numberOfColumns);

            // Unfilled corner.
            if (matrixLocations[numberOfRows * numberOfColumns - 1] == 0)
                matrixLocations[numberOfRows * numberOfColumns - 1] = matrixLocations[numberOfRows * numberOfColumns - numberOfColumns - 2] = 1;

        }

        // Calculate and append error correction code, and if necessary interleave.
        static void AddErrorCorrection(List<byte> dataStream, int dataBlocks, int eccBlocks, bool skew)
        {
            int byteCount = dataStream.Count;
            int blocks = (byteCount + 2) / dataBlocks;

            // Allocate space for error correction bytes.
            int totalEccBlocks = blocks * eccBlocks;
            for (int i = 0; i < totalEccBlocks; i++)
                dataStream.Add(0);

            ReedSolomon.RSInitialise(0x12d, eccBlocks, 1);
            for (int b = 0; b < blocks; b++)
            {
                byte[] dataCodewords = new byte[dataBlocks];
                byte[] eccCodewords = new byte[eccBlocks];
                int length = 0;

                for (int n = b; n < byteCount; n += blocks)
                    dataCodewords[length++] = dataStream[n];

                ReedSolomon.RSEncode(length, dataCodewords, eccCodewords);
                length = eccBlocks - 1;	// comes back reversed

                for (int n = b; n < totalEccBlocks; n += blocks)
                {
                    if (skew)
                    {
                        // Rotate ecc data to make 144x144 size symbols acceptable.
                        // See http://groups.google.com/group/postscriptbarcode/msg/5ae8fda7757477da
                        if (b < 8)
                            dataStream[byteCount + n + 2] = eccCodewords[length--];

                        else
                            dataStream[byteCount + n - 8] = eccCodewords[length--];
                    }

                    else
                        dataStream[byteCount + n] = eccCodewords[length--];
                }
            }
        }

        private bool SpecialX12(int position, int inputLength)
        {
            /* Annex P section (r)(6)(ii)(I)
               "If one of the three X12 terminator/separator characters first
               occurs in the yet to be processed data before a non-X12 character..."  */

            int i;
            int nonX12Position = 0;
            int specialX12Position = 0;
            bool returnValue = false;

            for (i = position; i < inputLength; i++)
            {
                if (nonX12Position == 0)
                {
                    if (!IsX12(barcodeData[i]))
                        nonX12Position = i;
                }

                if (specialX12Position == 0)
                {
                    if ((barcodeData[i] == (char)13) || (barcodeData[i] == '*') || (barcodeData[i] == '>'))
                        specialX12Position = i;
                }
            }

            if ((nonX12Position != 0) && (specialX12Position != 0))
            {
                if (specialX12Position < nonX12Position)
                    returnValue = true;
            }

            return returnValue;
        }

        static bool IsX12(char value)
        {
            if (value == 13 || value == 32 || value == 42 || value == 62)
                return true;

            if (((value >= '0') && (value <= '9')) || ((value >= 'A') && (value <= 'Z')))
                return true;

            return false;
        }

        private int LookAheadTest(int position, int current_mode, int inputLength)
        {
            float asciiCount, c40Count, textCount, x12Ccount, edifactCount, b256Count, bestCount;
            int bestScheme = 0;
            int sp;

            /* step (j) */
            if (current_mode == ASCII)
            {
                asciiCount = 0.0f;
                c40Count = 1.0f;
                textCount = 1.0f;
                x12Ccount = 1.0f;
                edifactCount = 1.0f;
                b256Count = 1.25f;
            }

            else
            {
                asciiCount = 1.0f;
                c40Count = 2.0f;
                textCount = 2.0f;
                x12Ccount = 2.0f;
                edifactCount = 2.0f;
                b256Count = 2.25f;
            }

            switch (current_mode)
            {
                case C40:
                    c40Count = 0.0f;
                    break;
                case TEXT:
                    textCount = 0.0f;
                    break;
                case X12:
                    x12Ccount = 0.0f;
                    break;
                case EDIFACT:
                    edifactCount = 0.0f;
                    break;
                case BASE256:
                    b256Count = 0.0f;
                    break;
            }

            sp = position;

            do
            {
                if (sp == (inputLength - 1))
                {
                    // At the end of data ... step (k)
                    asciiCount = (float)Math.Ceiling(asciiCount);
                    b256Count = (float)Math.Ceiling(b256Count);
                    edifactCount = (float)Math.Ceiling(edifactCount);
                    textCount = (float)Math.Ceiling(textCount);
                    x12Ccount = (float)Math.Ceiling(x12Ccount);
                    c40Count = (float)Math.Ceiling(c40Count);

                    bestCount = c40Count;
                    bestScheme = C40; // (k)(7)

                    if (x12Ccount < bestCount)
                    {
                        bestCount = x12Ccount;
                        bestScheme = X12; // (k)(6)
                    }

                    if (textCount < bestCount)
                    {
                        bestCount = textCount;
                        bestScheme = TEXT; // (k)(5)
                    }

                    if (edifactCount < bestCount)
                    {
                        bestCount = edifactCount;
                        bestScheme = EDIFACT; // (k)(4)
                    }

                    if (b256Count < bestCount)
                    {
                        bestCount = b256Count;
                        bestScheme = BASE256; // (k)(3)
                    }

                    if (asciiCount <= bestCount)
                        bestScheme = ASCII; // (k)(2)
                }

                else
                {
                    /* ascii ... step (l) */
                    if (Char.IsDigit(barcodeData[sp]))
                        asciiCount += 0.5F; // (l)(1)

                    else
                    {
                        if (barcodeData[sp] > 127)
                            asciiCount = (float)Math.Ceiling(asciiCount) + 2.0f; // (l)(2)

                        else
                            asciiCount = (float)Math.Ceiling(asciiCount) + 1.0f; // (l)(3)
                    }

                    /* c40 ... step (m) */
                    if (barcodeData[sp] == ' ' || Char.IsNumber(barcodeData[sp]) || Char.IsUpper(barcodeData[sp]))
                        c40Count += (2.0F / 3.0F); // (m)(1)

                    else
                    {
                        if (barcodeData[sp] > 127)
                            c40Count += (8.0F / 3.0F); // (m)(2)

                        else
                            c40Count += (4.0F / 3.0F); // (m)(3)
                    }

                    /* text ... step (n) */
                    if (barcodeData[sp] == ' ' || Char.IsNumber(barcodeData[sp]) || Char.IsLower(barcodeData[sp]))
                        textCount += (2.0F / 3.0F); // (n)(1)

                    else
                    {
                        if (barcodeData[sp] > 127)
                            textCount += (8.0F / 3.0F); // (n)(2)

                        else
                            textCount += (4.0F / 3.0F); // (n)(3)
                    }

                    /* x12 ... step (o) */
                    if (IsX12(barcodeData[sp]))
                        x12Ccount += (2.0F / 3.0F); // (o)(1)

                    else
                    {
                        if (barcodeData[sp] > 127)
                            x12Ccount += (13.0F / 3.0F); // (o)(2)

                        else
                            x12Ccount += (10.0F / 3.0F); // (o)(3)
                    }

                    /* edifact ... step (p) */
                    if ((barcodeData[sp] >= ' ') && (barcodeData[sp] <= '^'))
                        edifactCount += (3.0F / 4.0F); // (p)(1)

                    else
                    {
                        if (barcodeData[sp] > 127)
                            edifactCount += (17.0F / 4.0F); // (p)(2)

                        else
                            edifactCount += (13.0F / 4.0F); // (p)(3)
                    }

                    if (barcodeData[sp] == '[')
                        edifactCount += 6.0F;

                    /* base 256 ... step (q) */
                    if (barcodeData[sp] == '[')
                        b256Count += 4.0F; // (q)(1)

                    else
                        b256Count += 1.0F; // (q)(2)
                }


                if (sp > (position + 3))
                {
                    /* 4 data characters processed ... step (r) */
                    /* step (r)(6) */
                    if (((c40Count + 1.0F) < asciiCount) &&
                            ((c40Count + 1.0F) < b256Count) &&
                            ((c40Count + 1.0F) < edifactCount) &&
                            ((c40Count + 1.0F) < textCount))
                    {
                        if (c40Count < x12Ccount)
                            bestScheme = C40;

                        if (c40Count == x12Ccount)
                        {
                            // Test (r)(6)(ii)(i)
                            if (SpecialX12(sp, inputLength))
                                bestScheme = X12;

                            else
                                bestScheme = C40;
                        }
                    }

                    /* step (r)(5) */
                    if (((x12Ccount + 1.0F) < asciiCount) &&
                            ((x12Ccount + 1.0F) < b256Count) &&
                            ((x12Ccount + 1.0F) < edifactCount) &&
                            ((x12Ccount + 1.0F) < textCount) &&
                            ((x12Ccount + 1.0F) < c40Count))
                        bestScheme = X12;

                    /* step (r)(4) */
                    if (((textCount + 1.0F) < asciiCount) &&
                            ((textCount + 1.0F) < b256Count) &&
                            ((textCount + 1.0F) < edifactCount) &&
                            ((textCount + 1.0F) < x12Ccount) &&
                            ((textCount + 1.0F) < c40Count))
                        bestScheme = TEXT;

                    /* step (r)(3) */
                    if (((edifactCount + 1.0F) < asciiCount) &&
                            ((edifactCount + 1.0F) < b256Count) &&
                            ((edifactCount + 1.0F) < textCount) &&
                            ((edifactCount + 1.0F) < x12Ccount) &&
                            ((edifactCount + 1.0F) < c40Count))
                        bestScheme = EDIFACT;

                    /* step (r)(2) */
                    if (((b256Count + 1.0F) <= asciiCount) ||
                            (((b256Count + 1.0F) < edifactCount) &&
                            ((b256Count + 1.0F) < textCount) &&
                            ((b256Count + 1.0F) < x12Ccount) &&
                            ((b256Count + 1.0F) < c40Count)))
                        bestScheme = BASE256;

                    /* step (r)(1) */
                    if (((asciiCount + 1.0F) <= b256Count) &&
                            ((asciiCount + 1.0F) <= edifactCount) &&
                            ((asciiCount + 1.0F) <= textCount) &&
                            ((asciiCount + 1.0F) <= x12Ccount) &&
                            ((asciiCount + 1.0F) <= c40Count))
                        bestScheme = ASCII;
                }

                sp++;
            } while (bestScheme == 0); // step (s)

            return bestScheme;
        }

        private bool ProcessInputData(List<byte> dataStream, ref int lastMode, int[] processBuffer, ref int processBytesLeft)
        {
            // Process data using ASCII, C40, Text, X12, EDIFACT or Base 256 modes as appropriate.
            // Supports encoding FNC1.

            int inputLength = barcodeData.Length;

            StringBuilder binaryString = new StringBuilder();
            int sourceIndex = 0;

            // Step (a).
            int currentMode = ASCII;
            int nextMode = ASCII;
            if (isGS1)   // FNC1.
            {
                dataStream.Add(232);
                binaryString.Append(" ");
            }

            /*if (optionReader)
            {

                if (isGS1)
                    throw new DataEncodingException("Cannot encode in GS1 mode and Reader Initialisation at the same time.");

                else
                {
                    // Reader Programming.
                    dataStream.Add(234);
                    binaryString.Append(" ");
                }
            }*/

            if (eci > 3)
            {
                dataStream.Add(241);                // Mode Indicator.
                dataStream.Add((byte)(eci + 1));    // ECI
            }

            /* Add support for Macro05/Macro06
            "[)>[RS]05[GS]...[RS][EOT]" -> CW 236
            "[)>[RS]06[GS]...[RS][EOT]" -> CW 237 */

            if (dataStream.Count == 0 && sourceIndex == 0 && inputLength >= 9
                 && barcodeData[0] == '[' && barcodeData[1] == ')' && barcodeData[2] == '>'
                 && barcodeData[3] == '\x1e' && barcodeData[4] == '0'
                 && (barcodeData[5] == '5' || barcodeData[5] == '6')
                 && barcodeData[6] == '\x1d'
                 && barcodeData[inputLength - 2] == '\x1e' && barcodeData[inputLength - 1] == '\x04')
            {
                // Output macro Codeword
                if (barcodeData[5] == '5')
                    dataStream.Add(236);

                else
                    dataStream.Add(237);

                binaryString.Append(" ");
                // Remove macro characters from input string
                sourceIndex = 7;
                inputLength -= 2;
            }

            while (sourceIndex < inputLength)
            {
                currentMode = nextMode;
                // Step (b) - ASCII encodation.
                if (currentMode == ASCII)
                {
                    nextMode = ASCII;
                    if (sourceIndex + 1 < inputLength && Char.IsDigit(barcodeData[sourceIndex]) && Char.IsDigit(barcodeData[sourceIndex + 1]))
                    {
                        byte value = (byte)((10 * (barcodeData[sourceIndex] - '0')) + (barcodeData[sourceIndex + 1] - '0') + 130);
                        dataStream.Add(value);
                        binaryString.Append(" ");
                        sourceIndex += 2;
                    }

                    else
                    {
                        nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);
                        if (nextMode != ASCII)
                        {
                            switch (nextMode)
                            {
                                case C40:
                                    dataStream.Add(230);
                                    break;

                                case TEXT:
                                    dataStream.Add(239);
                                    break;

                                case X12:
                                    dataStream.Add(238);
                                    break;

                                case EDIFACT:
                                    dataStream.Add(240);
                                    break;

                                case BASE256:
                                    dataStream.Add(231);
                                    break;
                            }

                            binaryString.Append(" ");
                        }

                        else
                        {
                            if (barcodeData[sourceIndex] > 127)
                            {
                                dataStream.Add(235);                // FNC4.
                                dataStream.Add((byte)((barcodeData[sourceIndex] - 128) + 1));
                                binaryString.Append("  ");
                            }

                            else
                            {
                                if (isGS1 && (barcodeData[sourceIndex] == '['))
                                    dataStream.Add(232);            //FNC1.

                                else
                                    dataStream.Add((byte)(barcodeData[sourceIndex] + 1));

                                binaryString.Append(" ");
                            }

                            sourceIndex++;
                        }
                    }
                }

                // Step (c) C40 encodation.
                if (currentMode == C40)
                {
                    int shiftSet, value;
                    nextMode = C40;

                    if (processBytesLeft == 0)
                        nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);

                    if (nextMode != C40)
                    {
                        dataStream.Add(254);
                        binaryString.Append(" ");
                        nextMode = ASCII;
                    }

                    else
                    {
                        if (barcodeData[sourceIndex] > 127)
                        {
                            processBuffer[processBytesLeft++] = 1;
                            processBuffer[processBytesLeft++] = 30;      // Upper Shift.
                            shiftSet = C40Shift[barcodeData[sourceIndex] - 128];
                            value = C40Values[barcodeData[sourceIndex] - 128];
                        }

                        else
                        {
                            shiftSet = C40Shift[barcodeData[sourceIndex]];
                            value = C40Values[barcodeData[sourceIndex]];
                        }

                        if (isGS1 && (barcodeData[sourceIndex] == '['))
                        {
                            shiftSet = 2;
                            value = 27;     // FNC1.
                        }

                        if (shiftSet != 0)
                            processBuffer[processBytesLeft++] = shiftSet - 1;

                        processBuffer[processBytesLeft++] = value;

                        if (processBytesLeft >= 3)
                        {
                            int intValue = (1600 * processBuffer[0]) + (40 * processBuffer[1]) + (processBuffer[2]) + 1;
                            dataStream.Add((byte)(intValue / 256));
                            dataStream.Add((byte)(intValue % 256));
                            binaryString.Append("  ");
                            processBuffer[0] = processBuffer[3];
                            processBuffer[1] = processBuffer[4];
                            processBuffer[2] = processBuffer[5];
                            processBuffer[3] = 0;
                            processBuffer[4] = 0;
                            processBuffer[5] = 0;
                            processBytesLeft -= 3;
                        }

                        sourceIndex++;
                    }
                }

                // Step (d) Text encodation.
                if (currentMode == TEXT)
                {
                    int shiftSet, value;
                    nextMode = TEXT;

                    if (processBytesLeft == 0)
                        nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);

                    if (nextMode != TEXT)
                    {
                        dataStream.Add(254);
                        binaryString.Append(" ");   // Unlatch.
                        nextMode = ASCII;
                    }

                    else
                    {
                        if (barcodeData[sourceIndex] > 127)
                        {
                            processBuffer[processBytesLeft++] = 1;
                            processBuffer[processBytesLeft++] = 30;    // Upper shift.
                            shiftSet = TextShift[barcodeData[sourceIndex] - 128];
                            value = TextValues[barcodeData[sourceIndex] - 128];
                        }

                        else
                        {
                            shiftSet = TextShift[barcodeData[sourceIndex]];
                            value = TextValues[barcodeData[sourceIndex]];
                        }

                        if (isGS1 && (barcodeData[sourceIndex] == '['))
                        {
                            shiftSet = 2;
                            value = 27; // FNC1.
                        }

                        if (shiftSet != 0)
                            processBuffer[processBytesLeft++] = shiftSet - 1;

                        processBuffer[processBytesLeft++] = value;

                        if (processBytesLeft >= 3)
                        {
                            int intValue = (1600 * processBuffer[0]) + (40 * processBuffer[1]) + (processBuffer[2]) + 1;
                            dataStream.Add((byte)(intValue / 256));
                            dataStream.Add((byte)(intValue % 256));
                            binaryString.Append("  ");
                            processBuffer[0] = processBuffer[3];
                            processBuffer[1] = processBuffer[4];
                            processBuffer[2] = processBuffer[5];
                            processBuffer[3] = 0;
                            processBuffer[4] = 0;
                            processBuffer[5] = 0;
                            processBytesLeft -= 3;
                        }

                        sourceIndex++;
                    }
                }

                // Step (e) X12 encodation.
                if (currentMode == X12)
                {
                    int value = 0;
                    nextMode = X12;

                    if (processBytesLeft == 0)
                        nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);

                    if (nextMode != X12)
                    {
                        dataStream.Add(254);
                        binaryString.Append(" ");   // Unlatch.
                        nextMode = ASCII;
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
                            value = (barcodeData[sourceIndex] - '0') + 4;

                        if ((barcodeData[sourceIndex] >= 'A') && (barcodeData[sourceIndex] <= 'Z'))
                            value = (barcodeData[sourceIndex] - 'A') + 14;

                        processBuffer[processBytesLeft++] = value;
                        if (processBytesLeft >= 3)
                        {
                            int intValue = (1600 * processBuffer[0]) + (40 * processBuffer[1]) + (processBuffer[2]) + 1;
                            dataStream.Add((byte)(intValue / 256));
                            dataStream.Add((byte)(intValue % 256));
                            binaryString.Append("  ");
                            processBuffer[0] = processBuffer[3];
                            processBuffer[1] = processBuffer[4];
                            processBuffer[2] = processBuffer[5];
                            processBuffer[3] = 0;
                            processBuffer[4] = 0;
                            processBuffer[5] = 0;
                            processBytesLeft -= 3;
                        }

                        sourceIndex++;
                    }
                }

                // Step (f) EDIFACT encodation.
                if (currentMode == EDIFACT)
                {
                    int value = 0;
                    nextMode = EDIFACT;

                    if (processBytesLeft == 3)
                        nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);

                    if (nextMode != EDIFACT)
                    {
                        processBuffer[processBytesLeft++] = 31;
                        nextMode = ASCII;
                    }

                    else
                    {
                        if ((barcodeData[sourceIndex] >= '@') && (barcodeData[sourceIndex] <= '^'))
                            value = barcodeData[sourceIndex] - '@';

                        if ((barcodeData[sourceIndex] >= ' ') && (barcodeData[sourceIndex] <= '?'))
                            value = barcodeData[sourceIndex];

                        processBuffer[processBytesLeft++] = value;
                        sourceIndex++;
                    }

                    if (processBytesLeft >= 4)
                    {
                        dataStream.Add((byte)((processBuffer[0] << 2) + ((processBuffer[1] & 0x30) >> 4)));
                        dataStream.Add((byte)(((processBuffer[1] & 0x0f) << 4) + ((processBuffer[2] & 0x3c) >> 2)));
                        dataStream.Add((byte)(((processBuffer[2] & 0x03) << 6) + processBuffer[3]));
                        binaryString.Append("   ");
                        processBuffer[0] = processBuffer[4];
                        processBuffer[1] = processBuffer[5];
                        processBuffer[2] = processBuffer[7];
                        processBuffer[3] = processBuffer[7];
                        processBuffer[4] = 0;
                        processBuffer[5] = 0;
                        processBuffer[6] = 0;
                        processBuffer[7] = 0;
                        processBytesLeft -= 4;
                    }
                }

                // Step (g) Base 256 encodation.
                if (currentMode == BASE256)
                {
                    nextMode = LookAheadTest(sourceIndex, currentMode, inputLength);
                    if (nextMode == BASE256)
                    {
                        if (barcodeData[sourceIndex] < 256)
                        {
                            dataStream.Add((byte)(barcodeData[sourceIndex]));
                            binaryString.Append("b");
                        }

                        else
                        {
                            dataStream.Add((byte)(barcodeData[sourceIndex] & 0xff));
                            binaryString.Append("b");
                            dataStream.Add((byte)(barcodeData[sourceIndex] >> 8));
                            binaryString.Append("b");
                        }

                        sourceIndex++;
                    }

                    else
                        nextMode = ASCII;
                }

                if (dataStream.Count > 1558)
                {
                    lastMode = currentMode;
                    return false;
                }
            }

            // Add length and randomising algorithm to b256.
            if (binaryString.Contains("b"))
            {
                int idx = 0;
                while (idx < dataStream.Count)
                {
                    if (binaryString[idx] == 'b')
                    {
                        if ((idx == 0) || ((idx != 0) && (binaryString[idx - 1] != 'b')))
                        {
                            // Start of binary data.
                            int binaryCount = 0;    // Length of b256 data.
                            for (int i = idx; i < binaryString.Length; i++)
                            {
                                if (binaryString[i] == 'b')
                                    binaryCount++;

                                else
                                    break;
                            }

                            if (binaryCount <= 249)
                            {
                                binaryString.Insert(idx, 'b');
                                dataStream.Insert(idx, (byte)binaryCount);
                            }

                            else
                            {
                                binaryString.Insert(idx, 'b');
                                binaryString.Insert(idx + 1, 'b');
                                dataStream.Insert(idx, (byte)((binaryCount / 250) + 249));
                                dataStream.Insert(idx + 1, (byte)(binaryCount % 250));
                            }
                        }
                    }

                    idx++;
                }

                for (int i = 0; i < dataStream.Count; i++)
                {
                    if (binaryString[i] == 'b')
                    {
                        int prn = ((149 * (i + 1)) % 255) + 1 + dataStream[i];
                        dataStream[i] = (prn <= 255) ? (byte)prn : (byte)(prn - 256);
                    }
                }
            }

            lastMode = currentMode;
            return true;
        }

        private void ProcessRemainingData(List<byte> dataStream, int lastMode, int symbolCharactersLeft, int[] processBuffer, int dataBytesLeft, int inputLength)
        {
            switch (lastMode)
            {
                case C40:
                case TEXT:
                    if (symbolCharactersLeft == dataBytesLeft)
                    {
                        if (dataBytesLeft == 1)   // 1 data character left to encode.
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));

                        if (dataBytesLeft == 2)   // 2 data characters left to encode.
                        {
                            // Pad with shift 1 value (0) and encode as double.
                            int intValue = (1600 * processBuffer[0]) + (40 * processBuffer[1]) + 1;
                            dataStream.Add((byte)(intValue / 256));
                            dataStream.Add((byte)(intValue % 256));
                        }
                    }

                    if (symbolCharactersLeft > dataBytesLeft)
                    {
                        dataStream.Add(254);    // Unlatch.
                        if (dataBytesLeft == 1 || (dataBytesLeft == 2 && processBuffer[0] < 3))
                            // Encode the last byte as ascii.
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));

                        else if (dataBytesLeft == 2)
                        {
                            // Encode last 2 bytes as ascii.
                            dataStream.Add((byte)(barcodeData[inputLength - 2] + 1));
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));
                        }
                    }
                    break;

                case X12:
                    if (symbolCharactersLeft == dataBytesLeft)
                    {
                        if (dataBytesLeft == 1)   // 1 data character left to encode.
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));

                        if (dataBytesLeft == 2)
                        {
                            // Encode last 2 bytes as ascii.
                            dataStream.Add((byte)(barcodeData[inputLength - 2] + 1));
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));
                        }
                    }

                    if (symbolCharactersLeft > dataBytesLeft)
                    {
                        dataStream.Add(254);    // Unlatch.
                        if (dataBytesLeft == 1)
                            // Encode the last byte as ascii.
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));

                        if (dataBytesLeft == 2)
                        {
                            // Encode last 2 bytes as ascii.
                            dataStream.Add((byte)(barcodeData[inputLength - 2] + 1));
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));
                        }
                    }
                    break;

                case EDIFACT:
                    if (symbolCharactersLeft <= 2)  // Unlatch not required!
                    {
                        if (dataBytesLeft == 1)
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));

                        if (dataBytesLeft == 2)
                        {
                            dataStream.Add((byte)(barcodeData[inputLength - 2] + 1));
                            dataStream.Add((byte)(barcodeData[inputLength - 1] + 1));
                        }
                    }

                    else
                    {
                        // Append edifact unlatch value (31) and empty buffer.
                        if (dataBytesLeft == 0)
                            dataStream.Add((byte)(31 << 2));

                        // Append edifact unlatch value (31) and encode as triple.
                        if (dataBytesLeft == 1)
                        {
                            dataStream.Add((byte)((processBuffer[0] << 2) + ((31 & 0x30) >> 4)));
                            dataStream.Add((byte)((31 & 0x0f) << 4));
                            dataStream.Add((byte)0);
                        }

                        if(dataBytesLeft == 2)
                        {
                            dataStream.Add((byte)((processBuffer[0] << 2) + ((processBuffer[1] & 0x30) >> 4)));
                            dataStream.Add((byte)(((processBuffer[1] & 0x0f) << 4) + ((31 & 0x3c) >> 2)));
                            dataStream.Add((byte)(((31 & 0x03) << 6)));
                        }

                        if(dataBytesLeft == 3)
                        {
                            dataStream.Add((byte)((processBuffer[0] << 2) + ((processBuffer[1] & 0x30) >> 4)));
                            dataStream.Add((byte)(((processBuffer[1] & 0x0f) << 4) + ((processBuffer[2] & 0x3c) >> 2)));
                            dataStream.Add((byte)(((processBuffer[2] & 0x03) << 6) + 31));
                        }

                    }

                    break;
            }
        }

        static void AddPadding(List<byte> dataStream, int padLength)
        {
            int prn;

            for (int i = padLength; i > 0; i--)
            {
                if (i == padLength)
                    dataStream.Add(129);

                else
                {
                    prn = ((149 * (dataStream.Count + 1)) % 253) + 130;
                    dataStream.Add((prn <= 254) ? (byte)prn : (byte)(prn - 254));
                }
            }
        }
    }
}