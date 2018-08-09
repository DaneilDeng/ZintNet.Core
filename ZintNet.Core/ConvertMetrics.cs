/* ConvertMetrics.cs - Scale conversions for ZintNet */

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
    ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
    LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
    OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
    SUCH DAMAGE.
 */

namespace DisplayMetrics
{
    /// <summary>
    /// Class for converting display scales.
    /// </summary>
    public abstract class Convert
    {
        private const float milliMetersPerInch = 25.4f; // 25.4 millimeter per inch

        /// <summary>
        /// Convert millimeters to pixels for a given display resolution.
        /// </summary>
        /// <param name="millimeter">measurement in millimeters.</param>
        /// <param name="dpi">display resolution</param>
        /// <returns>pixels.</returns>
        public static float MillimetersToPixels(float millimeter, float dpi)
        {
            return (millimeter / milliMetersPerInch) * dpi;
        }

        /// <summary>
        /// Convert inches to pixels for a given display resolution.
        /// </summary>
        /// <param name="inches">measurement in inches(mils)</param>
        /// <param name="dpi">display resolution</param>
        /// <returns>pixels</returns>
        /// <remarks>inches in tenths</remarks>
        public static float InchesToPixels(float inches, float dpi)
        {
            return inches * dpi;
        }

        /// <summary>
        /// Convert pixels to millimeters for a given display resolution.
        /// </summary>
        /// <param name="pixels">number of pixels</param>
        /// <param name="dpi">display resolution</param>
        /// <returns>millimeters</returns>
        public static float PixelsToMillimeter(float pixels, float dpi)
        {
            return (pixels / dpi) * milliMetersPerInch;
        }

        /// <summary>
        /// Convert pixels to inches for a given display resolution.
        /// </summary>
        /// <param name="pixels">number of pixles</param>
        /// <param name="dpi">display resolution</param>
        /// <returns></returns>
        public static float PixelsToInches(float pixels, float dpi)
        {
            return pixels / dpi;
        }
    };
}