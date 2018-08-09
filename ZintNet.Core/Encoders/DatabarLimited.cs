/* DatabarLimited.cs Handles Databar Limited 1D + composite 2D symbol */

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal partial class DatabarEncoder
    {
        # region Tables
        // RSS Limited Tables.
        static int[] OddEvenTable = {
			17, 6, 9, 3, 28, 183064,
            13, 5, 13, 4, 728, 637000,
            9, 3, 17, 6, 6454, 180712,
            15, 5, 11, 4, 203, 490245,
			11, 4, 15, 5, 2408,	488824,
            19, 8, 7, 1, 1, 17094,
            7, 1, 19, 8, 16632, 16632};

        static int[] LeftWeights = { 1, 3, 9, 27, 81, 65, 17, 51, 64, 14, 42, 37, 22, 66 };
        static int[] RightWeights = { 20, 60, 2, 6, 18, 54, 73, 41, 34, 13, 39, 28, 84, 74 };
        static byte[] FinderPatternsLimited = {
	        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 3, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 3, 2, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 3, 2, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 3, 1, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 3, 2, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 3, 1, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 3, 1, 1, 1,
	        1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 3, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 2, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 3, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 2, 2, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 1, 2, 2, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 3, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 2, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 2, 1, 2, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 3, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 2, 2, 1, 2, 1, 1, 1,
	        1, 2, 1, 1, 1, 2, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 2, 1, 2, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 3, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 3, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 1, 3, 2, 1, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 1, 2, 3, 1, 1, 2, 1, 1,
	        1, 1, 1, 2, 1, 1, 1, 1, 3, 1, 1, 2, 1, 1,
	        1, 2, 1, 1, 1, 1, 1, 1, 3, 1, 1, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 3, 1, 1,
	        1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 2, 2, 1, 1,
	        1, 1, 1, 1, 1, 1, 2, 1, 1, 3, 2, 1, 1, 1,
	        1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1,
	        1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1,
	        1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1,
	        1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 2, 1, 1, 1,
	        1, 1, 1, 2, 1, 2, 2, 1, 1, 1, 2, 1, 1, 1,
	        1, 1, 1, 3, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1,
	        1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1,
	        1, 2, 1, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1,
	        1, 2, 1, 2, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1,
	        1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 3, 1, 1,
	        1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 2, 2, 1, 1,
	        1, 1, 1, 1, 2, 1, 1, 1, 1, 3, 2, 1, 1, 1,
	        1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 2, 1, 1,
	        1, 1, 1, 1, 2, 1, 1, 2, 1, 2, 2, 1, 1, 1,
	        1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1,
	        1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2, 1, 1,
	        1, 2, 1, 1, 2, 1, 1, 1, 1, 2, 2, 1, 1, 1,
	        1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1,
	        1, 2, 1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1,
	        1, 2, 1, 2, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1,
	        1, 3, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 3, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 1, 1, 3, 2, 1, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1, 1,
	        1, 1, 2, 1, 1, 1, 1, 3, 1, 1, 2, 1, 1, 1,
	        1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2, 2, 1, 1,
	        1, 1, 2, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 1,
	        1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1,
	        2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1,
	        2, 1, 1, 1, 1, 1, 1, 1, 1, 3, 2, 1, 1, 1,
	        2, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1,
	        2, 1, 1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1, 1,
	        2, 1, 1, 1, 1, 1, 1, 3, 1, 1, 2, 1, 1, 1,
	        2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 1,
	        2, 1, 1, 1, 1, 2, 1, 2, 1, 1, 2, 1, 1, 1,
	        2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1};
        # endregion

        private void DatabarLimited()
        {
            // GS1 DataBar Limited.
            const double COMPOSITE_FLAG = 2015133531096.0;
            const double LEFT_MULTIPLIER = 2013571.0;
            const int K = 7;
            const int PARITY_MODULAS = 89;

            double dataValue;
            int value, numberValue;
            int elementN, elementMax, parity;
            int characterValue, leftCharacterValue;
            int index;
            byte[] bars = new byte[46];
            int inputLength = barcodeData.Length;

            if (inputLength > 13)
                throw new InvalidDataLengthException("Databar Limited: Input too long.");

            for (int i = 0; i < inputLength; i++)
            {
                if (i == 0)
                {
                    if (barcodeData[i] != '0' && barcodeData[i] != '1')
                        throw new InvalidDataException("DataBar Limited: Requires the first character to be '0' or '1'.");
                }

                if (!Char.IsDigit(barcodeData[i]))
                    throw new InvalidDataException("Databar Limited: Non numeric data in input.");
            }

            dataValue = double.Parse(barcodeMessage, CultureInfo.CurrentCulture);

            if (isCompositeSymbol)
                dataValue += COMPOSITE_FLAG;

            // Calculate left (high order) symbol half value.
            characterValue = leftCharacterValue = (int)(dataValue / LEFT_MULTIPLIER);

            // Get 1st character index into OddEvenTable.
            index = 0;
            while (characterValue >= OddEvenTable[index + 5])
            {
                characterValue -= OddEvenTable[index + 5];
                index += 6;
            }

            // Get odd elements N and Max.
            elementN = (int)OddEvenTable[index];
            elementMax = (int)OddEvenTable[index + 1];
            numberValue = value = characterValue / OddEvenTable[index + 4];

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, true);
            parity = 0;
            for (int i = 0; i < K; i++)
            {
                bars[2 + (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeights[i * 2] * RSSWidths[i];
                parity = parity % PARITY_MODULAS;
            }

            // Calculate even elements value.
            value = characterValue - (OddEvenTable[index + 4] * numberValue);
            elementN = OddEvenTable[index + 2];
            elementMax = OddEvenTable[index + 3];

            // Generate and store even element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[3 + (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeights[(i * 2) + 1] * RSSWidths[i];
                parity = parity % PARITY_MODULAS;
            }

            // Calculate right (low order) symbol half value.
            characterValue = (int)(dataValue - (leftCharacterValue * LEFT_MULTIPLIER));

            // Get 2nd char index into OddEvenTable.
            index = 0;
            while (characterValue >= OddEvenTable[index + 5])
            {
                characterValue -= OddEvenTable[index + 5];
                index += 6;
            }

            // Get odd elements N and Max.
            elementN = (int)OddEvenTable[index];
            elementMax = (int)OddEvenTable[index + 1];
            numberValue = value = (characterValue / OddEvenTable[index + 4]);

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, true);
            for (int i = 0; i < K; i++)
            {
                bars[30 + (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeights[i * 2] * RSSWidths[i];
                parity = parity % PARITY_MODULAS;
            }

            // Calculate even elements value:
            value = (int)(characterValue - (OddEvenTable[index + 4] * numberValue));
            elementN = (int)OddEvenTable[index + 2];
            elementMax = (int)OddEvenTable[index + 3];

            // Generate and store even element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[31 + (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeights[(i * 2) + 1] * RSSWidths[i];
                parity = parity % PARITY_MODULAS;
            }

            // Store parity character in bars.
            for (int i = 0; i < 14; i++)
                bars[16 + i] = FinderPatternsLimited[(parity * 14) + i];

            // Insert guard bars.
            bars[0] = 1;
            bars[1] = 1;
            bars[44] = 1;
            bars[45] = 1;

            // Build the symbol.
            int symbolWidth = 0;
            int position = 0;
            bool latch = false;

            for (int i = 0; i < 46; i++)
                symbolWidth += bars[i];

            byte[] rowData = new byte[symbolWidth];

            for (int i = 0; i < 46; i++)
            {
                for (int j = 0; j < bars[i]; j++)
                {
                    if (latch)
                        rowData[position] = 1;

                    position++;
                }

                latch = !latch;
            }

            SymbolData symbolData = new SymbolData(rowData, 10.0f);    // Databar Limited height = 10X.
            Symbol.Add(symbolData);

            if (isCompositeSymbol)
                AddComposite(symbolWidth);
        }
    }
}