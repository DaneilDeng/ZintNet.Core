/* BitVector.cs - Managers a byte array as a vector of bits */

/*
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to ZXing Authors and Contributors.

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
using System.Text;

namespace ZintNet
{
    /// <summary>
    /// Class for storing and retreving bit data.
    /// </summary>
    /// <author>satorux@google.com (Satoru Takabayashi) - creator
    /// </author>
    /// <author>dswitkin@google.com (Daniel Switkin) - ported from C++
    /// </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    /// <author>Milton Neal milton200954@gmail.com Ported to C# and added additional methods for manipulating bit data.
    /// </author>

    public sealed class BitVector
    {
        private const int DefaultSize = 32;
        private int sizeInBits;
        private byte[] array;

        /// <summary>
        /// Gets a copy of the vector as a byte array.
        /// </summary>
        /// <returns>a copy of the vector array</returns>
        public byte[] ToByteArray()
        {
            int length = SizeInBytes;
            byte[] arrayCopy = new byte[length];
            Array.Copy(array, arrayCopy, length);
            return arrayCopy;
        }

        /// <summary>
        /// Gets the size of the vector in bits.
        /// </summary>
        public int SizeInBits
        {
            get { return sizeInBits; }
        }

        /// <summary>
        /// Gets the size of the vector in bytes.
        /// </summary>
        public int SizeInBytes
        {
            get { return (sizeInBits + 7) >> 3; }
        }

        /// <summary>
        /// Get the current capacity of the vector.
        /// </summary>
        public int Capacity
        {
            get { return array.Length; }
        }

        /// <summary>
        /// Initializes a new instance of BitVector class.
        /// </summary>
        public BitVector()
        {
            sizeInBits = 0;
            array = new byte[DefaultSize];
        }

        /// <summary>
        /// Initializes a new instance of BitVector class of a given size.
        /// </summary>
        /// <param name="size">size in bytes of the vector</param>
        public BitVector(int size)
        {
            sizeInBits = 0;
            array = new byte[size];
        }

        /// <summary>
        /// Returns the bit value at a specified bit index.
        /// </summary>
        /// <param name="index">index to the bit to be returned</param>
        private byte ValueAtIndex(int index)
        {
            if (index < 0 || index >= sizeInBits)
                throw new ArgumentOutOfRangeException("Index outside the range of the vector: " + index);

            int byteValue = array[index >> 3] & 0xff;
            return (byte)((byteValue >> (7 - (index & 0x7))) & 1);
        }

        /// <summary>
        /// Sets the bit at a specified bit index.
        /// </summary>
        /// <param name="index">index at which to set the bit</param>
        /// <param name="value">value to set the bit</param>
        private void SetAtIndex(int index, byte value)
        {
            if (index < 0 || index >= sizeInBits)
                throw new ArgumentOutOfRangeException("Index outside the range of the vector: " + index);

            if (!(value == 0 || value == 1))
                throw new System.ArgumentException("Invalid value for bit vector: " + value);

            int byteValue = array[index >> 3] & 0xff;
            if (value == 1)
                byteValue = (byte)(byteValue | (1 << (7 - (index & 0x7))));     // Left-shift 1, then bitwise OR

            else
                byteValue = (byte)(byteValue & ~(1 << (7 - (index & 0x7))));    // Left-shift 1, then take complement, then bitwise AND

            array[index >> 3] = (byte)(byteValue & 0xff);
        }

        /// <summary>
        /// Inserts a bit into the vector.
        /// </summary>
        /// <param name="index">index to insert the bit</param>
        /// <param name="value">value of the bit</param>
        public void Insert(int index, byte value)
        {
            byte bit;

            if (!(value == 0 || value == 1))
                throw new System.ArgumentException("Invalid value for bit vector: " + value);

            AppendBit(0);
            int position = sizeInBits - 1;
            while (position > index)
            {
                bit = ValueAtIndex(position - 1);
                SetAtIndex(position, bit);
                position--;
            }

            SetAtIndex(index, value);
        }

        /// <summary>
        /// Removes bits from the vector.
        /// </summary>
        /// <param name="index">index within the vector to remove the bit</param>
        /// <param name="count">number of bits to remove</param>
        public void RemoveBits(int index, int count)
        {
            if ((index + count) > sizeInBits)
                throw new ArgumentOutOfRangeException("Index outside the range of the vector: " + (index + count).ToString(CultureInfo.CurrentCulture));

            int bitsToShift = sizeInBits - (count + index);
            if ((index + count) < sizeInBits)
            {
                for (int i = index; i < bitsToShift; i++)
                {
                    byte bit = ValueAtIndex(i + count);
                    SetAtIndex(i, bit);
                }
            }

            sizeInBits -= count;
        }

        /// <summary>
        /// Appends one bit to the vector.
        /// </summary>
        /// <param name="value">the bit value to add</param>
        public void AppendBit(byte value)
        {
            if (!(value == 0 || value == 1))
                throw new System.ArgumentException("Invalid value for bit vector: " + value);

            int numBitsInLastByte = sizeInBits & 0x7;
            if (numBitsInLastByte == 0)
            {
                AppendByte(0);
                sizeInBits -= 8;
            }

            // Modify the last byte.
            array[sizeInBits >> 3] |= (byte)((value << (7 - numBitsInLastByte)));
            sizeInBits++;
        }

        /// <summary>
        /// /// <summary>
        /// Appends bits to the vector.
        /// </summary>
        /// <remarks>
        /// - appendBits(0x00, 1) adds 0.
        /// - appendBits(0x00, 4) adds 0000.
        /// - appendBits(0xff, 8) adds 11111111.
        /// </remarks>
        /// </summary>
        /// <param name="value">value to add</param>
        /// <param name="numberOfBits">number of bits from 'value'</param>
        public void AppendBits(int value, int numberOfBits)
        {
            if (numberOfBits < 0 || numberOfBits > 32)
                throw new System.ArgumentException("Number of bits must be between 0 and 32");

            int numberOfBitsLeft = numberOfBits;
            while (numberOfBitsLeft > 0)
            {
                // Optimization for byte-oriented appending.
                if ((sizeInBits & 0x07) == 0 && numberOfBitsLeft >= 8)
                {
                    byte newByte = (byte)((value >> (numberOfBitsLeft - 8)) & 0xff);
                    AppendByte(newByte);
                    numberOfBitsLeft -= 8;
                }

                else
                {
                    byte bit = (byte)((value >> (numberOfBitsLeft - 1)) & 0x01);
                    AppendBit(bit);
                    --numberOfBitsLeft;
                }
            }
        }

        /// <summary>
        /// Clears the contents of the bit vector and resets it's size to 0.
        /// </summary>
        public void Clear()
        {
            int length = sizeInBits;
            for (int i = 0; i < SizeInBytes; i++)
                this.array[i] = 0;

            sizeInBits = 0;
        }

        /// <summary>
        /// Appends the contents of a bit vector to the current vector.
        /// </summary><param type=""></param>
        /// <param name="bitVector">the bit vector to be appended</param>
        public void AppendBitVector(BitVector bitVector)
        {
            int size = bitVector.SizeInBits;
            for (int i = 0; i < size; ++i)
                AppendBit(bitVector.ValueAtIndex(i));
        }

        /// <summary>
        /// Modify the bit vector by XOR'ing with another vector.
        /// </summary>
        /// <param name="bitVector">vector to Xor with</param>

        public void Xor(BitVector bitVector)
        {
            if (sizeInBits != bitVector.SizeInBits)
                throw new System.ArgumentException("Bit Vector sizes don't match");

            int size = this.SizeInBytes;
            for (int i = 0; i < size; ++i)
            {
                // The last byte could be incomplete (i.e. not have 8 bits in
                // it) but there is no problem since 0 XOR 0 == 0.
                array[i] ^= bitVector.array[i];
            }
        }

        /// <summary>
        /// Bit Vector indexer.
        /// </summary>
        /// <param name="index">index position within the vector</param>
        /// <returns>value at the index</returns>
        public byte this[int index]
        {
            get { return ValueAtIndex(index); }
            set { SetAtIndex(index, value); }
        }

        /// <summary>
        /// Builds a string representation of the bit vector.
        /// </summary>
        /// <returns></returns>
        public override System.String ToString()
        {
            StringBuilder result = new StringBuilder(sizeInBits);
            for (int i = 0; i < sizeInBits; ++i)
                result.Append(ValueAtIndex(i) == 0 ? '0' : '1');

            return result.ToString();
        }

        /// <summary>
        /// Appends a byte to the end of the vector.
        /// </summary>
        /// <param name="value">the byte value to be added</param>
        private void AppendByte(byte value)
        {
            if ((sizeInBits >> 3) == array.Length)
                System.Array.Resize(ref array, array.Length << 1);

            array[sizeInBits >> 3] = value;
            sizeInBits += 8;
        }
    }
}