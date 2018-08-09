/* DataBarEncoder.cs - Handles DataBar based 1D + composite 2D symbols */

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
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace ZintNet.Core.Encoders
{
    internal partial class DatabarEncoder : SymbolEncoder
    {
        # region Tables
        private static int[] RSSWidths;

        // Databar 14 Tables.
        static int[] Table154 = {
		    10, 7, 5, 2, 4, 336, 
			8, 5, 7, 4, 20, 700, 
			6, 3, 9, 6, 48, 480, 
			4, 1, 11, 8, 81, 81};

        static int[] Table164 = {
		    12, 8, 4, 1, 1, 161, 
			10, 6, 6, 3, 10, 800, 
			8, 4, 8, 5, 34, 1054, 
			6, 3, 10, 6, 70, 700, 
			4, 1, 12, 8, 126, 126};

        static byte[] LeftParityTable = {
		    3, 8, 2, 
		    3, 5, 5, 
		    3, 3, 7, 
		    3, 1, 9, 
		    2, 7, 4, 
		    2, 5, 6, 
		    2, 3, 8, 
		    1, 5, 7, 
		    1, 3, 9};

        static int[] LeftWeightsTable = { 1, 3, 9, 27, 2, 6, 18, 54, 4, 12, 36, 29, 8, 24, 72, 58 };
        static int[] RightWeightsTable = { 16, 48, 65, 37, 32, 17, 51, 74, 64, 34, 23, 69, 49, 68, 46, 59 };

        # endregion

        private int segments;

        public DatabarEncoder(Symbology symbology, string barcodeMessage, string compositeMessage, CompositeMode compositeMode, int segments)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.compositeMessage = compositeMessage;
            this.segments = segments;
            this.compositeMode = compositeMode;
            if (!string.IsNullOrEmpty(compositeMessage))
                isCompositeSymbol = true;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            switch (symbolId)
            {
                case Symbology.DatabarOmni:
                case Symbology.DatabarOmniStacked:
                case Symbology.DatabarStacked:
                case Symbology.DatabarTruncated:
                    barcodeData = MessagePreProcessor.NumericOnlyParser(barcodeMessage);
                    Databar();
                    if(symbolId == Symbology.DatabarOmni || symbolId == Symbology.DatabarTruncated)
                        SetHRText();

                    break;

                case Symbology.DatabarLimited:
                    barcodeData = MessagePreProcessor.MessageParser(barcodeMessage);
                    DatabarLimited();
                    SetHRText();
                    break;

                case Symbology.DatabarExpanded:
                case Symbology.DatabarExpandedStacked:
                    barcodeData = MessagePreProcessor.AIParser(barcodeMessage);
                    DatabarExpanded();
                    if (symbolId == Symbology.DatabarExpanded)
                    {
                        barcodeText = barcodeMessage;
                        barcodeText = barcodeText.Replace('[', '(');
                        barcodeText = barcodeText.Replace(']', ')');
                    }

                    break;
            }

            return Symbol;
        }

        private void Databar()
        {
            const double COMPOSITE_FLAG = 10000000000000.0;
            const double LEFT_MULTIPLIER = 4537077.0;
            const int SEMI_MULTIPLIER = 1597;
            const int K = 4;
            const int PARITY_MODULAS = 79;

            double dataValue;
            int value;
            int elementN, elementMax;
            int parity;
            int characterValue, leftCharacterValue, semiValue, oddSemiValue, numberValue;
            int index;
            byte[] bars = new byte[46];
            int inputLength = barcodeData.Length;

            if (inputLength > 13)
                throw new InvalidDataLengthException("GS1 Databar: Input too long.");

            dataValue = double.Parse(barcodeMessage, CultureInfo.CurrentCulture);
            if (isCompositeSymbol)
                dataValue += COMPOSITE_FLAG;

            // Calculate left (high order) symbol half value.
            characterValue = leftCharacterValue = (int)(dataValue / LEFT_MULTIPLIER);

            // Determine the 1st (left) character and get the 1st char odd elements value.
            semiValue = oddSemiValue = characterValue / SEMI_MULTIPLIER;

            // Get 1st char index into Table164.
            index = 0;
            while (semiValue >= Table164[index + 5])
            {
                semiValue -= Table164[index + 5];
                index += 6;
            }

            // Get odd elements N and max.
            elementN = Table164[index];
            elementMax = Table164[index + 1];
            numberValue = value = semiValue / Table164[index + 4];

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, true);
            parity = 0;
            for (int i = 0; i < K; i++)
            {
                bars[2 + (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeightsTable[i * 2] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate even elements value.
            value = semiValue - (Table164[index + 4] * numberValue);
            elementN = Table164[index + 2];
            elementMax = Table164[index + 3];

            // Generate and store even element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[3 + (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeightsTable[(i * 2) + 1] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Get the 2nd character value.
            semiValue = characterValue - (oddSemiValue * SEMI_MULTIPLIER);

            // Get 2nd character index into Table154.
            index = 0;
            while (semiValue >= Table154[index + 5])
            {
                semiValue -= Table154[index + 5];
                index += 6;
            }

            // Get even elements N and max.
            elementN = Table154[index];
            elementMax = Table154[index + 1];
            numberValue = value = semiValue / Table154[index + 4];

            // Generate and store even element widths of the 2nd character.
            GetRSSWidths(value, elementN, K, elementMax, true);
            for (int i = 0; i < K; i++)
            {
                bars[21 - (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeightsTable[(i * 2) + 1 + 8] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate 2nd character odd elements value.
            value = semiValue - (Table154[index + 4] * numberValue);
            elementN = Table154[index + 2];
            elementMax = Table154[index + 3];

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[22 - (i * 2)] = (byte)RSSWidths[i];
                parity += LeftWeightsTable[(i * 2) + 8] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate right (low order) symbol half value.
            characterValue = (int)(dataValue - ((double)leftCharacterValue * LEFT_MULTIPLIER));

            // Determine the 3rd character and get the 3rd character odd elements value.
            semiValue = oddSemiValue = characterValue / SEMI_MULTIPLIER;

            // Get 3rd character index into Table164.
            index = 0;
            while (semiValue >= Table164[index + 5])
            {
                semiValue -= Table164[index + 5];
                index += 6;
            }

            // Get odd elements N and max.
            elementN = Table164[index];
            elementMax = Table164[index + 1];
            numberValue = value = semiValue / Table164[index + 4];

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, true);
            for (int i = 0; i < K; i++)
            {
                bars[43 - (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeightsTable[i * 2] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate even elements value.
            value = semiValue - (Table164[index + 4] * numberValue);
            elementN = Table164[index + 2];
            elementMax = Table164[index + 3];

            // Generate and store even element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[42 - (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeightsTable[(i * 2) + 1] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Get the 4th character value
            semiValue = characterValue - (oddSemiValue * SEMI_MULTIPLIER);

            // Get 4th character index into Table154.
            index = 0;
            while (semiValue >= Table154[index + 5])
            {
                semiValue -= Table154[index + 5];
                index += 6;
            }

            // Get even elements N and max.
            elementN = Table154[index];
            elementMax = Table154[index + 1];
            numberValue = value = semiValue / Table154[index + 4];

            // Generate and store even element widths of the 4th character.
            GetRSSWidths(value, elementN, K, elementMax, true);
            for (int i = 0; i < K; i++)
            {
                bars[24 + (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeightsTable[(i * 2) + 1 + 8] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate 4th character odd elements value.
            value = semiValue - (Table154[index + 4] * numberValue);
            elementN = Table154[index + 2];
            elementMax = Table154[index + 3];

            // Generate and store odd element widths.
            GetRSSWidths(value, elementN, K, elementMax, false);
            for (int i = 0; i < K; i++)
            {
                bars[23 + (i * 2)] = (byte)RSSWidths[i];
                parity += RightWeightsTable[(i * 2) + 8] * RSSWidths[i];
                parity %= PARITY_MODULAS;
            }

            // Calculate finders
            if (parity >= 8)
                parity++;   // Avoid 0,8 doppelganger.

            if (parity >= 72)
                parity++;   // Avoid 8,0 doppelganger.

            int leftParity = parity / 9;
            int rightParity = parity % 9;

            // Store left (high order) parity character in the bars.
            for (int i = 0; i < 3; i++)
                bars[10 + i] = LeftParityTable[leftParity * 3 + i];

            // Store right (low order) parity character in the spaces.
            for (int i = 0; i < 3; i++)
                bars[35 - i] = LeftParityTable[rightParity * 3 + i];

            // Set fixed patterns.
            bars[13] = 1;
            bars[14] = 1;
            bars[31] = 1;
            bars[32] = 1;

            // Insert guard bars.
            bars[0] = 1;
            bars[1] = 1;
            bars[44] = 1;
            bars[45] = 1;


            // Build the symbol.
            byte[] rowData;
            SymbolData symbolData;
            int symbolWidth = 0;
            int position = 0;
            bool latch = false;

            if (symbolId == Symbology.DatabarOmni ||symbolId == Symbology.DatabarTruncated)
            {
                float rowHeight = 33.0f;    // Databar Omnidirectional = 33X.

                if (symbolId == Symbology.DatabarTruncated)
                    rowHeight = 13.0f;      // Truncated = 13X.

                for (int i = 0; i < 46; i++)
                    symbolWidth += bars[i];

                rowData = new byte[symbolWidth];

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

                symbolData = new SymbolData(rowData, rowHeight);
                Symbol.Add(symbolData);

                if (isCompositeSymbol)
                    AddComposite(symbolWidth);
            }

            if (symbolId == Symbology.DatabarStacked)
            {
                // Top row.
                for (int i = 0; i < 23; i++)
                    symbolWidth += bars[i];

                symbolWidth += 2;
                rowData = new byte[symbolWidth];
                for (int i = 0; i < 23; i++)
                {
                    for (int j = 0; j < bars[i]; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }

                rowData[position] = 1;
                symbolData = new SymbolData(rowData, 5.0f);     // Databar Stacked top row height = 5X.
                Symbol.Add(symbolData);

                // Bottom row.
                rowData = new byte[symbolWidth];
                rowData[0] = 1;
                position = 2;
                latch = true;
                for (int i = 23; i < 46; i++)
                {
                    for (int j = 0; j < bars[i]; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }

                symbolData = new SymbolData(rowData, 7.0f);    // Databar Stacked bottom row height = 7X.
                Symbol.Add(symbolData);

                // Separator pattern.
                rowData = new byte[46];
                for (int i = 4; i < 46; i++)
                {
                    if (Symbol[0].GetRowData()[i] == Symbol[1].GetRowData()[i])
                    {
                        if (Symbol[0].GetRowData()[i] == 0)
                            rowData[i] = 1;
                    }

                    else
                    {
                        if (rowData[i - 1] == 0)
                            rowData[i] = 1;
                    }
                }

                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(1, symbolData);

                if (isCompositeSymbol)
                    AddComposite(symbolWidth);
            }

            if (symbolId == Symbology.DatabarOmniStacked)
            {
                // Top row.
                for (int i = 0; i < 23; i++)
                    symbolWidth += bars[i];

                symbolWidth += 2;
                rowData = new byte[symbolWidth];

                for (int i = 0; i < 23; i++)
                {
                    for (int j = 0; j < bars[i]; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }

                rowData[position] = 1;
                symbolData = new SymbolData(rowData, 25.0f);   // Databar Omnidirectional Stacked top row height = 25X.
                Symbol.Add(symbolData);

                // Bottom row.
                rowData = new byte[symbolWidth];
                rowData[0] = 1;
                position = 2;
                latch = true;
                for (int i = 23; i < 46; i++)
                {
                    for (int j = 0; j < bars[i]; j++)
                    {
                        if (latch)
                            rowData[position] = 1;

                        position++;
                    }

                    latch = !latch;
                }

                symbolData = new SymbolData(rowData, 25.0f);   // Databar Omnidirectional Stacked bottom row height = 25X.
                Symbol.Add(symbolData);

                // Middle separator.
                rowData = new byte[symbolWidth];
                for (int i = 5; i < 46; i += 2)
                    rowData[i] = 1;

                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(1, symbolData);

                // Top separator.
                rowData = new byte[symbolWidth];
                for (int i = 4; i < 46; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                        rowData[i] = 1;
                }

                latch = true;
                for (int i = 17; i < 33; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                    {
                        rowData[i] = (latch) ? (byte)1 : (byte)0;
                        latch = !latch;
                    }

                    else
                    {
                        rowData[i] = 0;
                        latch = true;
                    }
                }

                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(1, symbolData);

                // Bottom separator.
                rowData = new byte[symbolWidth];
                for (int i = 4; i < 46; i++)
                {
                    if (Symbol[3].GetRowData()[i] == 0)
                        rowData[i] = 1;
                }

                latch = true;
                for (int i = 16; i < 32; i++)
                {
                    if (Symbol[3].GetRowData()[i] == 0)
                    {
                        rowData[i] = (latch) ? (byte)1 : (byte)0;
                        latch = !latch;
                    }

                    else
                    {
                        rowData[i] = 0;
                        latch = true;
                    }
                }

                symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Insert(3, symbolData);

                if (isCompositeSymbol)
                    AddComposite(symbolWidth);
            }
        }

        private void AddComposite(int symbolWidth)
        {
            bool latch;
            byte[] rowData = new byte[symbolWidth];

            // Separator pattern for composite symbol.
            for (int i = 4; i < symbolWidth - 4; i++)
            {
                if (Symbol[0].GetRowData()[i] == 0)
                    rowData[i] = 1;
            }

            if (symbolWidth != 74)
            {
                latch = true;
                for (int i = 16; i < 32; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                    {
                        rowData[i] = (latch) ? (byte)1 : (byte)0;
                        latch = !latch;
                    }

                    else
                    {
                        rowData[i] = 0;
                        latch = true;
                    }
                }
            }

            if (symbolWidth == 96)
            {
                latch = true;
                for (int i = 63; i < 78; i++)
                {
                    if (Symbol[0].GetRowData()[i] == 0)
                    {
                        rowData[i] = (latch) ? (byte)1 : (byte)0;
                        latch = !latch;
                    }

                    else
                    {
                        rowData[i] = 0;
                        latch = true;
                    }
                }
            }

            SymbolData symbolData = new SymbolData(rowData, 1.0f);
            Symbol.Insert(0, symbolData);
            CompositeEncoder.AddComposite(symbolId, compositeMessage, Symbol, compositeMode, symbolWidth);
        }

        private static void GetRSSWidths(int value, int n, int elements, int maxWidth, bool noNarrow)
        {
            int elementWidth;
            int mxwElement;
            int subVal;
            int narrowMask = 0;
            int bar;

            RSSWidths = new int[8];

            for (bar = 0; bar < elements - 1; bar++)
            {
                for (elementWidth = 1, narrowMask |= (1 << bar); ; elementWidth++, narrowMask &= ~(1 << bar))
                {
                    // Get all combinations.
                    subVal = Combinations(n - elementWidth - 1, elements - bar - 2);

                    // Less combinations with no single-module element.
                    if (!noNarrow && (narrowMask == 0) && (n - elementWidth - (elements - bar - 1) >= elements - bar - 1))
                        subVal -= Combinations(n - elementWidth - (elements - bar), elements - bar - 2);

                    // Less combinations with elements > maxVal.
                    if (elements - bar - 1 > 1)
                    {
                        int lessVal = 0;
                        for (mxwElement = n - elementWidth - (elements - bar - 2); mxwElement > maxWidth; mxwElement--)
                            lessVal += Combinations(n - elementWidth - mxwElement - 1, elements - bar - 3);

                        subVal -= lessVal * (elements - 1 - bar);
                    }

                    else if ((n - elementWidth) > maxWidth)
                        subVal--;

                    value -= subVal;
                    if (value < 0)
                        break;
                }

                value += subVal;
                n -= elementWidth;
                RSSWidths[bar] = elementWidth;
            }

            RSSWidths[bar] = n;
        }

        private static int Combinations(int n, int r)
        {
            int maxDenom, minDenom;

            if (n - r > r)
            {
                minDenom = r;
                maxDenom = n - r;
            }

            else
            {
                minDenom = n - r;
                maxDenom = r;
            }

            int value = 1;
            int j = 1;

            for (int i = n; i > maxDenom; i--)
            {
                value *= i;
                if (j <= minDenom)
                {
                    value /= j;
                    j++;
                }
            }

            for (; j <= minDenom; j++)
                value /= j;

            return value;
        }

        private void SetHRText()
        {
            char checkDigit = CheckSum.Mod10CheckDigit(barcodeData);
            barcodeText = "(01)" + barcodeMessage.PadLeft(13, '0') + checkDigit.ToString();
        }
    }
}
