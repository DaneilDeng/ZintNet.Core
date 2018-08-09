using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ZintNet
{
    /// <summary>
    /// Build the symbols row data.
    /// </summary>
    internal static class SymbolBuilder
    {
        public static void ExpandSymbolRow(Collection<SymbolData> Symbol, StringBuilder rowPattern, float height)
        {
            List<byte> rowData = new List<byte>();
            bool latch = true;	    // Start with a bar.
            for (int i = 0; i < rowPattern.Length; i++)
            {
                int value = rowPattern[i] - '0';
                for (int j = 0; j < value; j++)
                {
                    if (latch)
                        rowData.Add(1);

                    else
                        rowData.Add(0);
                }

                latch = !latch;
            }

            SymbolData symbolData = new SymbolData(rowData.ToArray(), height);
            Symbol.Add(symbolData);
        }

        public static void BuildFourStateSymbol(Collection<SymbolData> Symbol, StringBuilder barPattern)
        {
            // Turn the symbol into a bar pattern.
            int index = 0;
            int patternLength = barPattern.Length;
            byte[] rowData1 = new byte[patternLength * 2];
            byte[] rowData2 = new byte[patternLength * 2];
            byte[] rowData3 = new byte[patternLength * 2];
            SymbolData symbolData;

            for (int i = 0; i < patternLength; i++)
            {
                if ((barPattern[i] == '1') || (barPattern[i] == '0'))
                    rowData1[index] = 1;

                rowData2[index] = 1;
                if ((barPattern[i] == '2') || (barPattern[i] == '0'))
                    rowData3[index] = 1;

                index += 2;
            }

            symbolData = new SymbolData(rowData1, 4.0f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData2, 2.5f);
            Symbol.Add(symbolData);
            symbolData = new SymbolData(rowData3, 4.0f);
            Symbol.Add(symbolData);
        }
    }
}
