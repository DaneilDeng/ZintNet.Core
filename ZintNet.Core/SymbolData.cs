/* SymbolData.cs - Type class for holding symbol row data */

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

namespace ZintNet
{
    /// <summary>
    /// Class for defining a row of barcode data.
    /// </summary>
    internal sealed class SymbolData
    {
        private float height;
        private byte[] data;

        /// <summary>
        /// Gets the height of the module in the row.
        /// </summary>
        public float RowHeight
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the number of elements in the row.
        /// </summary>
        public int RowCount
        {
            get { return data.Length; }
        }

        /// <summary>
        /// Sets the row data.
        /// </summary>
        public byte[] RowData
        {
            set { data = value; }
        }

        /// <summary>
        /// Creates a new instance of SymbolData object.
        /// </summary>
        /// <param name="data">row data</param>
        /// <param name="height">element row height</param>
        public SymbolData(byte[] data, float height)
        {
            this.data = data;
            this.height = height;
        }

        /// <summary>
        /// Creates a new instance of SymbolData object.
        /// </summary>
        /// <param name="data">row data</param>
        public SymbolData(byte[] data)
        {
            this.data = data;
            this.height = 0.0f; // The module height will be defined by the barcode height property. Generally for linear sysbols eg. Code128
        }

        /// <summary>
        /// Gets the row data.
        /// </summary>
        /// <returns>a copy of the row data</returns>
        public byte[] GetRowData()
        {
            return (byte[])data.Clone();
        }
    };
}