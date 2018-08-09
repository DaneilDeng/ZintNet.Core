using System;
using System.Collections.Generic;
using System.Text;

namespace ZintNet.Core
{
    /// <summary>
    /// Extended methods for arrays.
    /// </summary>
    abstract class ArrayExtensions
    {
        /// <summary>
        /// Converts a character array to uppercase.
        /// </summary>
        /// <param name="data">input array</param>
        /// <returns>converted array</returns>
        public static char[] ToUpper(char[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > 96 && data[i] < 123)
                    data[i] -= (char)32;
            }

            return data;
        }

        // Insert integer into the array at position.
        public static int[] Insert(int[] data, int position, int value)
        {
            int length = data.Length + 1;

            if (position > length)
                throw new ArgumentOutOfRangeException("position");

            Array.Resize(ref data, length);
            for (int i = length - 1; i > position; i--)
                data[i] = data[i - 1];

            data[position] = value;
            return data;
        }

        /// <summary>
        /// Inserts a byte into the array at a specified position.
        /// </summary>
        /// <param name="data">array to insert the data byte</param>
        /// <param name="position">position to insert the byte</param>
        /// <param name="value">byte value to insert</param>
        /// <returns>the new array</returns>
        public static byte[] Insert(byte[] data, int position, byte value)
        {
            int length = data.Length + 1;

            if (position > length)
                throw new ArgumentOutOfRangeException("position");

            System.Array.Resize(ref data, length);
            for (int i = length - 1; i > position; i--)
                data[i] = data[i - 1];

            data[position] = value;
            return data;
        }

        public static byte[] Insert(byte[] data, int position, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                data = Insert(data, position, bytes[i]);
                position++;
            }

            return data;
        }

        // Insert character into the array at position.
        public static char[] Insert(char[] data, int position, char value)
        {
            int length = data.Length + 1;

            if (position > length)
                throw new ArgumentOutOfRangeException("position");

            Array.Resize(ref data, length);
            for (int i = length - 1; i > position; i--)
                data[i] = data[i - 1];

            data[position] = value;
            return data;
        }

        // Insert characters into the array at position.
        public static char[] Insert(char[] data, int position, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                data = Insert(data, position, chars[i]);
                position++;
            }

            return data;
        }

        public static char[] Insert(char[] data, int position, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                data = Insert(data, position, str[i]);
                position++;
            }

            return data;
        }

        // Removes the character at position from the array.
        public static char[] Remove(char[] data, int position)
        {
            int length = data.Length;

            if (position > length)
                throw new ArgumentOutOfRangeException("position");

            for (int i = position; i < length - 1; i++)
                data[i] = data[i + 1];

            Array.Resize(ref data, length - 1);
            return data;
        }

        public static byte[] Remove(byte[] data, int position)
        {
            int length = data.Length;

            if (position > length)
                throw new ArgumentOutOfRangeException("position");

            for (int i = position; i < length - 1; i++)
                data[i] = data[i + 1];

            System.Array.Resize(ref data, length - 1);
            return data;
        }
    }
}
