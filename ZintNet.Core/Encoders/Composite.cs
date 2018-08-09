using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Extensions;

namespace ZintNet.Core.Encoders
{
    internal static class CompositeEncoder
    {
        private static int[,] pwr928 = new int[69, 7];
        private static Symbology hostSymbol;
        private static char[] compositeData;
        private static Collection<SymbolData> encodedData;
        private static int linearWidth;

        public static void AddComposite(Symbology symbology, string message, Collection<SymbolData> encodedLinearData,
            CompositeMode compositeMode, int linearSymbolWidth)
        {
            int dataColumns = 0;
            int eccLevel = 0;
            int compositeShiftCount = 0;
            int linearShiftCount = 0;
            BitVector bitStream;

            hostSymbol = symbology;
            encodedData = encodedLinearData;
            linearWidth = linearSymbolWidth;
            
            compositeData = MessagePreProcessor.AIParser(message);
            int inputLength = compositeData.Length;

            if (inputLength > 2990)
                throw new InvalidDataLengthException("2D Component: Input data too long.");

            if ((compositeMode == CompositeMode.CCC) && (symbology != Symbology.Code128))
                throw new DataEncodingException("2D Component: Invalid mode, CC-C only valid with a GS1-128 linear component.");

            switch (symbology)
            {
                // Determine width of 2D component according to ISO/IEC 24723 Table 1.
                case Symbology.EAN8:
                    dataColumns = 3;
                    linearShiftCount = 3;
                    if (compositeMode == CompositeMode.CCB)
                        linearShiftCount = 13;

                    break;

                case Symbology.EAN13:
                    dataColumns = 4;
                    linearShiftCount = 2;
                    break;

                case Symbology.UPCE:
                    dataColumns = 2;
                    linearShiftCount = 2;
                    break;

                case Symbology.UPCA:
                    dataColumns = 4;
                    linearShiftCount = 3;
                    break;

                case Symbology.Code128:
                    dataColumns = 4;
                    if (compositeMode == CompositeMode.CCC)
                        linearShiftCount = 7;

                    break;

                case Symbology.DatabarOmni:
                case Symbology.DatabarTruncated:
                    dataColumns = 4;
                    linearShiftCount = 4;
                    break;

                case Symbology.DatabarLimited:
                    dataColumns = 3;
                    compositeShiftCount = 0;
                    break;

                case Symbology.DatabarExpanded:
                    dataColumns = 4;
                    compositeShiftCount = 1;
                    while ((encodedData[1].GetRowData()[compositeShiftCount - 1] == 0) && (encodedData[1].GetRowData()[compositeShiftCount] == 1))
                        compositeShiftCount++;

                    break;

                case Symbology.DatabarStacked:
                case Symbology.DatabarOmniStacked:
                    dataColumns = 2;
                    compositeShiftCount = 0;
                    break;

                case Symbology.DatabarExpandedStacked:
                    dataColumns = 4;
                    compositeShiftCount = 1;
                    while ((encodedData[1].GetRowData()[compositeShiftCount - 1] == 0) && (encodedData[1].GetRowData()[compositeShiftCount] == 1))
                        compositeShiftCount++;

                    break;
            }

            if (linearShiftCount > 0)
                ShiftLinearHost(linearShiftCount);

            bitStream = BitStreamEncoder.CompositeBitStream(symbology, compositeData, ref compositeMode, ref dataColumns, ref eccLevel, linearWidth);
            if (bitStream == null)
                throw new InvalidDataLengthException();

            switch (compositeMode)
            {
                // Note that eccLevel is only relevant to CC-C.
                case CompositeMode.CCA:
                    CompositeA(bitStream, dataColumns, compositeShiftCount);
                    break;

                case CompositeMode.CCB:
                    CompositeB(bitStream, dataColumns, compositeShiftCount);
                    break;

                case CompositeMode.CCC:
                    CompositeC(bitStream, dataColumns, eccLevel);
                    break;
            }
        }

        /// <summary>
        /// Initialize pwr928 encoding table.
        /// </summary>
        private static void Initialize928()
        {
            int[] cw = new int[7];
            cw[6] = 1;

            for (int i = 0; i < 7; i++)
                pwr928[0, i] = cw[i];

            for (int j = 1; j < 69; j++)
            {
                int v;
                int i;
                for (v = 0, i = 6; i >= 1; i--)
                {
                    v = (2 * cw[i]) + (v / 928);
                    pwr928[j, i] = cw[i] = v % 928;
                }

                pwr928[j, 0] = cw[0] = (2 * cw[0]) + (v / 928);
            }

            return;
        }

        // Converts a bit stream to base 928 values, codeWords[0] is highest order.
        private static int Encode928(BitVector binaryData, int[] dataStream)
        {
            int bitLength = binaryData.SizeInBits;
            int index = 0;
            int dataCodewords = 0;
            int codewordCount;
            int bitCount;

            for (int b = 0; b < bitLength; b += 69, index += 7)
            {
                bitCount = Math.Min(bitLength - b, 69);
                codewordCount = bitCount / 10 + 1;
                dataCodewords += codewordCount;
                for (int i = 0; i < codewordCount; i++)
                    dataStream[index + i] = 0;

                for (int i = 0; i < bitCount; i++)
                {
                    if(binaryData[b + bitCount - i - 1] == 1)
                    {
                        for (int j = 0; j < codewordCount; j++)
                            dataStream[index + j] += pwr928[i, j + 7 - codewordCount];
                    }
                }

                for (int i = codewordCount - 1; i > 0; i--)
                {
                    // Add "carries".
                    dataStream[index + i - 1] += dataStream[index + i] / 928;
                    dataStream[index + i] %= 928;
                }
            }

            return (dataCodewords);
        }

        private static void CompositeA(BitVector bitStream, int dataColumns, int compositeShiftCount)
        {
            // CC-A 2D component.
            int variant = 0;
            StringBuilder binaryString;
            BitVector bitPattern;
            int[] dataStream = new int[28];

            Initialize928();
            // Encode dataStream from bit stream
            int dataCodewords = Encode928(bitStream, dataStream);
            switch (dataColumns)
            {
                case 2:
                    switch (dataCodewords)
                    {
                        case 6: variant = 0; break;
                        case 8: variant = 1; break;
                        case 9: variant = 2; break;
                        case 11: variant = 3; break;
                        case 12: variant = 4; break;
                        case 14: variant = 5; break;
                        case 17: variant = 6; break;
                    }
                    break;

                case 3:
                    switch (dataCodewords)
                    {
                        case 8: variant = 7; break;
                        case 10: variant = 8; break;
                        case 12: variant = 9; break;
                        case 14: variant = 10; break;
                        case 17: variant = 11; break;
                    }
                    break;

                case 4:
                    switch (dataCodewords)
                    {
                        case 8: variant = 12; break;
                        case 11: variant = 13; break;
                        case 14: variant = 14; break;
                        case 17: variant = 15; break;
                        case 20: variant = 16; break;
                    }
                    break;
            }

            int rowCount = PDF417Tables.CCAVariants[variant];
            int eccCodewords = PDF417Tables.CCAVariants[17 + variant];
            int offset = PDF417Tables.CCAVariants[34 + variant];

            // Reed-Solomon error correction.
            int[] eccStream = new int[eccCodewords];
            for (int i = 0; i < dataCodewords; i++)
            {
                int total = (dataStream[i] + eccStream[eccCodewords - 1]) % 929;
                for (int j = eccCodewords - 1; j > 0; j--)
                    eccStream[j] = (eccStream[j - 1] + 929 - (total * PDF417Tables.CCACoefficients[offset + j]) % 929) % 929;

                eccStream[0] = (929 - (total * PDF417Tables.CCACoefficients[offset]) % 929) % 929;
            }

            for (int j = 0; j < eccCodewords; j++)
            {
                if (eccStream[j] != 0)
                    eccStream[j] = 929 - eccStream[j];
            }

            for (int i = eccCodewords - 1; i >= 0; i--)
                dataStream[dataCodewords++] = eccStream[i];

            // Place data into table.
            int leftRAPStart = PDF417Tables.CCARAPTable[variant];
            int centreRAPStart = PDF417Tables.CCARAPTable[variant + 17];
            int rightRAPStart = PDF417Tables.CCARAPTable[variant + 34];
            int startCluster = PDF417Tables.CCARAPTable[variant + 51] / 3;

            int leftRAP = leftRAPStart;
            int centreRAP = centreRAPStart;
            int rightRAP = rightRAPStart;
            int cluster = startCluster; // Cluster can be 0, 1 or 2 for cluster(0), cluster(3) and cluster(6).

            binaryString = new StringBuilder();
            bitPattern = new BitVector();
            int[] buffer = new int[dataColumns + 1];

            for (int row = 0; row < rowCount; row++)
            {
                offset = 929 * cluster;
                for (int j = 0; j < buffer.Length; j++)
                    buffer[j] = 0;

                for (int j = 0; j < dataColumns; j++)
                    buffer[j + 1] = dataStream[row * dataColumns + j];

                // Copy the data into code string.
                if(dataColumns != 3)
                    binaryString.Append(PDF417Tables.RowAddressPattern[leftRAP]);

                binaryString.Append("1");
                binaryString.Append(PDF417Tables.EncodingPatterns[offset + buffer[1]]);
                binaryString.Append("1");

                if (dataColumns == 3)
                    binaryString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (dataColumns >= 2)
                {
                    binaryString.Append("1");
                    binaryString.Append(PDF417Tables.EncodingPatterns[offset + buffer[2]]);
                    binaryString.Append("1");
                }

                if (dataColumns == 4)
                    binaryString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (dataColumns >= 3)
                {
                    binaryString.Append("1");
                    binaryString.Append(PDF417Tables.EncodingPatterns[offset + buffer[3]]);
                    binaryString.Append("1");
                }

                if (dataColumns == 4)
                {
                    binaryString.Append("1");
                    binaryString.Append(PDF417Tables.EncodingPatterns[offset + buffer[4]]);
                    binaryString.Append("1");
                }

                binaryString.Append(PDF417Tables.RowAddressPattern[rightRAP]);
                binaryString.Append("1"); // Stop.

                // The code string is a mixture of letters and numbers.
                bool latch = true;
                for (int i = 0; i < binaryString.Length; i++)
                {
                    if ((binaryString[i] >= '0') && (binaryString[i] <= '9'))
                    {
                        int value = (int)(binaryString[i] - '0');
                        bitPattern.AppendBits((latch) ? 0xffff : 0, value);
                        latch = !latch;
                    }

                    else
                    {
                        int position = PDF417Tables.PDFSet.IndexOf(binaryString[i]);
                        if (position >= 0 && position < 32)
                            bitPattern.AppendBits(position, 5);
                    }
                }

                int bitPatternLength = bitPattern.SizeInBits;
                if (hostSymbol == Symbology.Code128)
                {
                    if(linearWidth > bitPatternLength)
                        compositeShiftCount = (linearWidth - bitPatternLength) / 2;

                    else
                        compositeShiftCount = 0;
                }

                byte[] rowData = new byte[bitPatternLength + compositeShiftCount];
                for (int i = 0; i < bitPatternLength; i++)
                    rowData[i + compositeShiftCount] = bitPattern[i];

                SymbolData symbolData = new SymbolData(rowData, 2);
                encodedData.Insert(row, symbolData);

                // Clear data and set up RAPs and cluster for next row.
                binaryString.Clear();
                bitPattern.Clear();

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

        private static void CompositeB(BitVector bitStream, int dataColumns, int compositeShiftCount)
        {
            // CC-B 2D component.
            int variant = 0;
            StringBuilder codeString = new StringBuilder();
            BitVector bitPattern = new BitVector();
            List<int> dataStream = new List<int>();

            // CC-B component requires codeword 920 in the first symbol character position (section 9a)
            dataStream.Add(920);
            ProcessByte(dataStream, bitStream);
            int dataStreamLength = dataStream.Count;

            // Calculate variant of the symbol to use and load values accordingly.
            if (dataColumns == 2)
            {
                variant = 13;
                if (dataStreamLength <= 33) { variant = 12; }
                if (dataStreamLength <= 29) { variant = 11; }
                if (dataStreamLength <= 24) { variant = 10; }
                if (dataStreamLength <= 19) { variant = 9; }
                if (dataStreamLength <= 13) { variant = 8; }
                if (dataStreamLength <= 8) { variant = 7; }
            }

            if (dataColumns == 3)
            {
                variant = 23;
                if (dataStreamLength <= 70) { variant = 22; }
                if (dataStreamLength <= 58) { variant = 21; }
                if (dataStreamLength <= 46) { variant = 20; }
                if (dataStreamLength <= 34) { variant = 19; }
                if (dataStreamLength <= 24) { variant = 18; }
                if (dataStreamLength <= 18) { variant = 17; }
                if (dataStreamLength <= 14) { variant = 16; }
                if (dataStreamLength <= 10) { variant = 15; }
                if (dataStreamLength <= 6) { variant = 14; }
            }

            if (dataColumns == 4)
            {
                variant = 34;
                if (dataStreamLength <= 108) { variant = 33; }
                if (dataStreamLength <= 90) { variant = 32; }
                if (dataStreamLength <= 72) { variant = 31; }
                if (dataStreamLength <= 54) { variant = 30; }
                if (dataStreamLength <= 39) { variant = 29; }
                if (dataStreamLength <= 30) { variant = 28; }
                if (dataStreamLength <= 24) { variant = 27; }
                if (dataStreamLength <= 18) { variant = 26; }
                if (dataStreamLength <= 12) { variant = 25; }
                if (dataStreamLength <= 8) { variant = 24; }
            }

            // Now we have the variant we can load the data - from here on the same as MicroPDF417 code.
            variant--;
            int rowCount = PDF417Tables.MicroVariants[variant + 34];
            int eccCodewords = PDF417Tables.MicroVariants[variant + 68];
            int dataCodewords = (dataColumns * rowCount) - eccCodewords;
            int padding = dataCodewords - dataStreamLength;
            int offset = PDF417Tables.MicroVariants[variant + 102]; // coefficient offset.

            // Add the padding.
            while (padding > 0)
            {
                dataStream.Add(900);
                padding--;
            }

            // Reed-Solomon error correction.
            dataStreamLength = dataStream.Count;
            int[] eccStream = new int[eccCodewords];
            int total = 0;

            for (int i = 0; i < dataStreamLength; i++)
            {
                total = (dataStream[i] + eccStream[eccCodewords - 1]) % 929;
                for (int j = eccCodewords - 1; j > 0; j--)
                    eccStream[j] = (eccStream[j - 1] + 929 - (total * PDF417Tables.MicroCoefficients[offset + j]) % 929) % 929;

                eccStream[0] = (929 - (total * PDF417Tables.MicroCoefficients[offset]) % 929) % 929;
            }

            for (int j = 0; j < eccCodewords; j++)
            {
                if (eccStream[j] != 0)
                    eccStream[j] = 929 - eccStream[j];
            }

            // Add the codewords to the data stream.
            for (int i = eccCodewords - 1; i >= 0; i--)
                dataStream.Add(eccStream[i]);

            dataStreamLength = dataStream.Count;

            // Get the RAP (Row Address Pattern) start values.
            int leftRAPStart = PDF417Tables.RowAddressTable[variant];
            int centreRAPStart = PDF417Tables.RowAddressTable[variant + 34];
            int rightRAPStart = PDF417Tables.RowAddressTable[variant + 68];
            int startCluster = PDF417Tables.RowAddressTable[variant + 102] / 3;

            // That's all values loaded, get on with the encoding.
            int leftRAP = leftRAPStart;
            int centreRAP = centreRAPStart;
            int rightRAP = rightRAPStart;
            int cluster = startCluster; // Cluster can be 0, 1 or 2 for cluster(0), cluster(3) and cluster(6).

            int[] buffer = new int[dataColumns + 1];
            for (int row = 0; row < rowCount; row++)
            {
                offset = 929 * cluster;
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;

                for (int i = 0; i < dataColumns; i++)
                    buffer[i + 1] = dataStream[row * dataColumns + i];

                // Copy the data into code string
                codeString.Append(PDF417Tables.RowAddressPattern[leftRAP]);
                codeString.Append("1");
                codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[1]]);
                codeString.Append("1");

                if (dataColumns == 3)
                    codeString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (dataColumns >= 2)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[2]]);
                    codeString.Append("1");
                }

                if (dataColumns == 4)
                    codeString.Append(PDF417Tables.CentreRowAddressPattern[centreRAP]);

                if (dataColumns >= 3)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[3]]);
                    codeString.Append("1");
                }

                if (dataColumns == 4)
                {
                    codeString.Append("1");
                    codeString.Append(PDF417Tables.EncodingPatterns[offset + buffer[4]]);
                    codeString.Append("1");
                }

                codeString.Append(PDF417Tables.RowAddressPattern[rightRAP]);
                codeString.Append("1"); // Stop.

                // Code string is a mixture of letters and numbers.
                bool latch = true;
                for (int i = 0; i < codeString.Length; i++)
                {
                    if ((codeString[i] >= '0') && (codeString[i] <= '9'))
                    {
                        int value = (int)(codeString[i] - '0');
                        bitPattern.AppendBits((latch) ? 0xffff : 0, value);
                        latch = !latch;
                    }

                    else
                    {
                        int position = PDF417Tables.PDFSet.IndexOf(codeString[i]);
                        if (position >= 0 && position < 32)
                            bitPattern.AppendBits(position, 5);
                    }
                }

                int bitPatternLength = bitPattern.SizeInBits;
                if (hostSymbol == Symbology.Code128)
                {
                    if (linearWidth > bitPatternLength)
                        compositeShiftCount = (linearWidth - bitPatternLength) / 2;

                    else
                        compositeShiftCount = 0;
                }

                byte[] rowData = new byte[bitPatternLength + compositeShiftCount];
                for (int i = 0; i < bitPatternLength; i++)
                    rowData[i + compositeShiftCount] = bitPattern[i];

                SymbolData symbolData = new SymbolData(rowData, 2);
                encodedData.Insert(row, symbolData);

                // Clear data and set up RAPs and Cluster for next row.
                codeString.Clear();
                bitPattern.Clear();

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

        private static void CompositeC(BitVector bitStream, int dataColumns, int eccLevel)
        {
            // CC-C 2D component - byte compressed PDF417.
            int eccCodewords;
            int offset;
            StringBuilder codeString = new StringBuilder();
            BitVector bitPattern = new BitVector();
            List<int> dataStream = new List<int>();

            dataStream.Add(0);      // Reserve for length descriptor;
            dataStream.Add(920);    // CC_C identifier.
            ProcessByte(dataStream, bitStream);
            dataStream[0] = dataStream.Count;

            eccCodewords = 1;
            for (int i = 0; i <= eccLevel; i++)
                eccCodewords *= 2;

            // Now take care of the Reed Solomon codes.
            if (eccCodewords == 2)
                offset = 0;

            else
                offset = eccCodewords - 2;

            int total = 0;
            int dataStreamLength = dataStream.Count;
            int[] eccStream = new int[eccCodewords];
            for (int i = 0; i < dataStreamLength; i++)
            {
                total = (dataStream[i] + eccStream[eccCodewords - 1]) % 929;
                for (int j = eccCodewords - 1; j > 0; j--)
                    eccStream[j] = (eccStream[j - 1] + 929 - (total * PDF417Tables.Coefficients[offset + j]) % 929) % 929;

                eccStream[0] = (929 - (total * PDF417Tables.Coefficients[offset]) % 929) % 929;
            }

            // Add the code words to the data stream.
            for (int i = eccCodewords - 1; i >= 0; i--)
                dataStream.Add((eccStream[i] != 0) ? 929 - eccStream[i] : 0);

            int rowCount = dataStream.Count / dataColumns;
            if (dataStream.Count % dataColumns != 0)
                rowCount++;

            int c1 = (rowCount - 1) / 3;
            int c2 = (eccLevel * 3) + ((rowCount - 1) % 3);
            int c3 = dataColumns - 1;

            // Encode each row.
            int[] buffer = new int[dataColumns + 2];

            for (int row = 0; row < rowCount; row++)
            {
                for (int j = 0; j < dataColumns; j++)
                    buffer[j + 1] = dataStream[row * dataColumns + j];

                eccCodewords = (row / 3) * 30;
                switch (row % 3)
                {
                    /* Follows this codePattern from US Patent 5,243,655: 
                    Row 0: L0 (row #, # of rows)         R0 (row #, # of columns)
                    Row 1: L1 (row #, security level)    R1 (row #, # of rows)
                    Row 2: L2 (row #, # of columns)      R2 (row #, security level)
                    Row 3: L3 (row #, # of rows)         R3 (row #, # of columns)
                    etc. */
                    case 0:
                        buffer[0] = eccCodewords + c1;
                        buffer[dataColumns + 1] = eccCodewords + c3;
                        break;

                    case 1:
                        buffer[0] = eccCodewords + c2;
                        buffer[dataColumns + 1] = eccCodewords + c1;
                        break;

                    case 2:
                        buffer[0] = eccCodewords + c3;
                        buffer[dataColumns + 1] = eccCodewords + c2;
                        break;
                }

                codeString.Append("+*"); // Start with a start char and a separator

                for (int j = 0; j <= dataColumns + 1; j++)
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

                SymbolData symbolData = new SymbolData(rowData, 3);
                encodedData.Insert(row, symbolData);

                // Clear data ready for next row.
                codeString.Clear();
                bitPattern.Clear();
            }
        }

        private static void ShiftLinearHost(int linearShiftCount)
        {
            IEnumerator<SymbolData> enumerator = encodedData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SymbolData symbolData = enumerator.Current;
                byte[] currentRow = symbolData.GetRowData();
                byte[] newRow = new byte[symbolData.RowCount + linearShiftCount];
                Array.Copy(currentRow, 0, newRow, linearShiftCount, currentRow.Length);
                symbolData.RowData = newRow;
            }
        }

        private static void ProcessByte(List<int> dataStream, BitVector bitStream)
        {
            byte[] byteData = bitStream.ToByteArray();
            int length = bitStream.SizeInBytes;
            int position = 0;
            int start = 0;
            int chunkLength = 0;
            ulong mantisa = 0;
            ulong total = 0;

            if (length == 1)
            {
                dataStream.Add(913);
                dataStream.Add((int)(byteData[start]));
            }

            else
            {
                // Select the switch for multiple of 6 bytes.
                if (length % 6 == 0)
                    dataStream.Add(924);

                else
                    dataStream.Add(901);

                while (position < length)
                {
                    chunkLength = length - position;
                    if (chunkLength >= 6)  // Take groups of 6.
                    {
                        chunkLength = 6;
                        position += chunkLength;
                        total = 0;

                        while (chunkLength-- != 0)
                        {
                            mantisa = (ulong)byteData[start++];
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
                        position += chunkLength;
                        while (chunkLength-- != 0)
                            dataStream.Add(byteData[start++]);
                    }
                }
            }
        }
    }
} 