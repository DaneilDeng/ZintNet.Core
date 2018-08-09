using System;

namespace ZintNet
{
    /// <summary>
    /// Class for binary math calculations on large numbers.
    /// </summary>
    internal static class BinaryMath
    {
        /// <summary>
        /// Binary addition.
        /// </summary>
        /// <param name="accumulator">result</param>
        /// <param name="register">operand</param>
        public static void BinaryAdd(short[] accumulator, short[] register)
        {
            int carry = 0;
            bool done;

            for (int i = 0; i < 112; i++)
            {
                done = false;
                if (((register[i] == 0) && (accumulator[i] == 0)) && ((carry == 0) && !done))
                {
                    accumulator[i] = 0;
                    carry = 0;
                    done = true;
                }

                if (((register[i] == 0) && (accumulator[i] == 0)) && ((carry == 1) && !done))
                {
                    accumulator[i] = 1;
                    carry = 0;
                    done = true;
                }

                if (((register[i] == 0) && (accumulator[i] == 1)) && ((carry == 0) && !done))
                {
                    accumulator[i] = 1;
                    carry = 0;
                    done = true;
                }

                if (((register[i] == 0) && (accumulator[i] == 1)) && ((carry == 1) && !done))
                {
                    accumulator[i] = 0;
                    carry = 1;
                    done = true;
                }

                if (((register[i] == 1) && (accumulator[i] == 0)) && ((carry == 0) && !done))
                {
                    accumulator[i] = 1;
                    carry = 0;
                    done = true;
                }

                if (((register[i] == 1) && (accumulator[i] == 0)) && ((carry == 1) && !done))
                {
                    accumulator[i] = 0;
                    carry = 1;
                    done = true;
                }

                if (((register[i] == 1) && (accumulator[i] == 1)) && ((carry == 0) && !done))
                {
                    accumulator[i] = 0;
                    carry = 1;
                    done = true;
                }

                if (((register[i] == 1) && (accumulator[i] == 1)) && ((carry == 1) && !done))
                {
                    accumulator[i] = 1;
                    carry = 1;
                    done = true;
                }
            }
        }

        /// <summary>
        /// Binary subtraction.
        /// </summary>
        /// <param name="accumulator">result</param>
        /// <param name="register">operand</param>
        public static void BinarySubtract(short[] accumulator, short[] register)
        {
            // 2's compliment subtraction.
            // Subtract buffer from accumulator and put answer in accumulator.
            short[] subtractBuffer = new short[112];

            for (int i = 0; i < 112; i++)
            {
                if (register[i] == 0)
                    subtractBuffer[i] = 1;

                else
                    subtractBuffer[i] = 0;
            }

            BinaryAdd(accumulator, subtractBuffer);
            subtractBuffer[0] = 1;

            for (int i = 1; i < 112; i++)
                subtractBuffer[i] = 0;

            BinaryAdd(accumulator, subtractBuffer);
        }

        /// <summary>
        /// Binary multipication.
        /// </summary>
        /// <param name="register">contents to be multiplied</param>
        /// <param name="data">number to multiply by</param>
        public static void BinaryMultiply(short[] register, string data)
        {
            short[] temporary = new short[112];
            short[] accumulator = new short[112];

            BinaryLoad(temporary, data);
            for(int i = 0; i < 102; i++)
            {
                if (temporary[i] == 1)
                    BinaryAdd(accumulator, register);

                ShiftUp(register);
            }

            Array.Copy(accumulator, register, 112);
        }

        /// <summary>
        /// Perform a binary shift left
        /// </summary>
        /// <param name="buffer">operand buffer</param>
        public static void ShiftDown(short[] buffer)
        {
            buffer[102] = 0;
            buffer[103] = 0;

            for (int i = 0; i < 102; i++)
                buffer[i] = buffer[i + 1];
        }

        /// <summary>
        /// Perform a binary shift right.
        /// </summary>
        /// <param name="buffer">operand buffer</param>
        public static void ShiftUp(short[] buffer)
        {
            for (int i = 102; i > 0; i--)
                buffer[i] = buffer[i - 1];

            buffer[0] = 0;
        }

        /// <summary>
        /// Perform a binary comparison between values.
        /// </summary>
        /// <param name="accumulator">value 1 for comparison</param>
        /// <param name="register">value 2 for comparison</param>
        /// <returns>1 if value1 is larger than value 2, else 0</returns>
        public static short IsLarger(short[] accumulator, short[] register)
        {
            bool latch = false;
            int index = 103;
            short larger = 0;

            do
            {
                if ((accumulator[index] == 1) && (register[index] == 0))
                {
                    latch = true;
                    larger = 1;
                }

                if ((accumulator[index] == 0) && (register[index] == 1))
                    latch = true;

                index--;
            } while ((latch == false) && (index > -1));

            return larger;
        }

        /// <summary>
        /// Convert a numeric string to a binary value.
        /// </summary>
        /// <param name="register">result</param>
        /// <param name="data">string to store as a binary value</param>
        public static void BinaryLoad(short[] register, string data)
        {
            int length = data.Length;
            short[] tempBuffer = new short[112];

            for(int i = 0; i < length; i++)
            {
                if(!Char.IsDigit(data[i]))
                    throw new ArgumentException("Data is not a numeric string value.");
            }

            Array.Clear(register, 0, 112);
            for (int index = 0; index < length; index++)
            {
                Array.Copy(register, tempBuffer, 112);

                for (int i = 0; i < 9; i++)
                    BinaryAdd(register, tempBuffer);

                Array.Clear(tempBuffer, 0, 112);

                for (int i = 0; i < 4; i++)
                {
                    if ((data[index] - '0' & (0x01 << i)) > 0)
                        tempBuffer[i] = 1;
                }

                BinaryAdd(register, tempBuffer);
            }
        }
    }
}
