/* ReedSolomon.cs - Handles Error Correction for symbols that utilise Reed Solomon */

/*
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>

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

namespace ZintNet
{
    internal static class ReedSolomon
    {
        private static int logmod;		// 2**symbolSize - 1
        private static int resultLength;
        private static int[] logTable;
        private static int[] alogTable;
        private static int[] rsPolynomial;

        public static void RSEncode(int length, byte[] data, byte[] result)
        {
	        for (int i = 0; i < length; i++)
            {
		        int m = result[resultLength - 1] ^ data[i];
		        for (int j = resultLength - 1; j > 0; j--)
                {
			        if (m != 0 && rsPolynomial[j] != 0)
				        result[j] = (byte)(result[j - 1] ^ alogTable[(logTable[m] + logTable[rsPolynomial[j]]) % logmod]);

			        else
				        result[j] = result[j - 1];
		        }

		        if (m != 0 && rsPolynomial[0] != 0)
			        result[0] = (byte) (alogTable[(logTable[m] + logTable[rsPolynomial[0]]) % logmod]);

		        else
			        result[0] = 0;
	        }
        }

        public static void RSEncode(int length, uint[] data, uint[] result)
        { 
            // The same as above but for larger bit lengths - Aztec code compatible.
	        for (int i = 0; i < length; i++)
            {
		        UInt32 m = result[resultLength - 1] ^ data[i];
		        for (int j = resultLength - 1; j > 0; j--)
                {
			        if (m != 0 && rsPolynomial[j] != 0)
				        result[j] = (UInt32) (result[j - 1] ^ alogTable[(logTable[m] + logTable[rsPolynomial[j]]) % logmod]);

			        else
				        result[j] = result[j - 1];
		        }

		        if (m != 0 && rsPolynomial[0] != 0)
			        result[0] = (UInt32) (alogTable[(logTable[m] + logTable[rsPolynomial[0]]) % logmod]);

		        else
			        result[0] = 0;
	        }
        }

        /// <summary>
        /// Initialises the Reed Solomon polynomials.
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="numberOfSymbols"></param>
        /// <param name="index"></param>
        public static void RSInitialise(int polynomial, int numberOfSymbols, int index)
        {
            resultLength = numberOfSymbols;

            int b;
            int size = 0;

            // Find the top bit, and hence the symbol size
            for (b = 1; b <= polynomial; b <<= 1)
                size++;

            size--;
            b >>= 1;

            // Build the log/alog tables
            logmod = (1 << size) - 1;
            logTable = new int[logmod + 1];
            alogTable = new int[logmod];
            rsPolynomial = new int[numberOfSymbols + 1];

            for (int p = 1, v = 0; v < logmod; v++)
            {
                alogTable[v] = p;
                logTable[p] = v;
                p <<= 1;
                if ((p & b) != 0)
                    p ^= polynomial;
            }

            rsPolynomial[0] = 1;
            for (int i = 1; i <= numberOfSymbols; i++)
            {
                rsPolynomial[i] = 1;
                for (int k = i - 1; k > 0; k--)
                {
                    if (rsPolynomial[k] != 0)
                        rsPolynomial[k] = alogTable[(logTable[rsPolynomial[k]] + index) % logmod];

                    rsPolynomial[k] ^= rsPolynomial[k - 1];
                }

                rsPolynomial[0] = alogTable[(logTable[rsPolynomial[0]] + index) % logmod];
                index++;
            }
        }
    }
}