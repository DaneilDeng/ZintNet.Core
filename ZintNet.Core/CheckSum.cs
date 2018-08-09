/* CheckSum.cs - Commonly used checksum calculators */

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
    internal static class CheckSum
    {
        /// <summary>
        /// Mod43 checksum calculator.
        /// </summary>
        /// <param name="data">data to calculate checksum</param>
        /// <returns>checksum</returns>
        public static char Mod43CheckDigit(char[] data)
        {
            int weight = 0;
            int length = data.Length;

            for (int i = 0; i < length; i++)
                weight += CharacterSets.Code39Set.IndexOf(data[i]);

            weight %= 43;
            return CharacterSets.Code39Set[weight];
        }

        public static char Mod10CheckDigit(char[] data, int length)
        {
            Array.Resize(ref data, length);
            char checkDigit = Mod10CheckDigit(data);
            return checkDigit;
        }

        public static char Mod10CheckDigit(char[] data)
        {
            // Start the checksum calculation from the right most position.
            int factor = 3;
            int weight = 0;
            int length = data.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                weight += (data[i] - '0') * factor;
                factor = (factor == 3) ? 1 : 3;
            }

            return (char)(((10 - (weight % 10)) % 10) + '0');
        }

        public static char OPCCCheckDigit(char[] data)
        {
            int factor = 2;
            int weight = 0;
            int length = data.Length;

            for (int i = 0; i < length; i++)
            {
                int value = (data[i] - '0') * factor;
                if (value > 9)
                {
                    weight += value / 10;
                    weight += value % 10;
                }

                else
                    weight += value;

                factor = (factor == 2) ? 1 : 2;
            }

            return (char)(((10 - (weight % 10)) % 10) + '0');
        }

        public static char Mod11CheckDigit(char[] data)
        {
            int value = 0;
            int weight = 0;
            int weightIndex = 1;
            int length = data.Length;

            for (int i = 0; i < length; i++)
            {
                value = (data[i] - '0');
                weight += (value * weightIndex);
                weightIndex++;
            }

            value = weight % 11;
            if (value == 10)
                return 'X';

            else
                return (char)(value + '0');
        }
    }
}
