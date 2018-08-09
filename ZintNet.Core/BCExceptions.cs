/* BCExceptions.cs - Exception handlers for ZintNet */

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
using System.Runtime.Serialization;

namespace ZintNet
{
    /// <summary>
    /// ZintNet.ZintNetDLLException class.
    /// </summary>
    [Serializable]
    public class ZintNetDLLException : System.Exception
    {
        private static string baseMessage = "ZintNet DLL error.";
        /// <summary>
        /// Initialses a new instance of ZintNet.ZintNetDLLException class.
        /// </summary>
        public ZintNetDLLException()
            : base()
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.ZintNetDLLException class with a specified error message.
        /// </summary>
        /// <param name="message">error message.</param>
        public ZintNetDLLException(string message)
            : base(baseMessage + Environment.NewLine + message)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.ZintNetDLLException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception.</param>
        public ZintNetDLLException(string message, Exception innerException)
            : base(baseMessage + Environment.NewLine + message, innerException)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.ZintNetDLLException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected ZintNetDLLException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// ZintNet.InvalidDataException class.
    /// </summary>
	[Serializable]
	public class InvalidDataException : System.Exception
	{
	private	static string baseMessage  = "Invalid data found in the barcode message.";
        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataException class.
        /// </summary>
		public InvalidDataException()
			: base( baseMessage )
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
		public InvalidDataException(string message) 
			: base(baseMessage + Environment.NewLine + message)
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
		public InvalidDataException(string message, Exception innerException) 
			: base(baseMessage + Environment.NewLine + message, innerException)
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
	    protected InvalidDataException(SerializationInfo info, StreamingContext context)
			    : base(info, context)
		{}
	}

    /// <summary>
    /// ZintNet.InvalidSymbolSizeException class.
    /// </summary>
    [Serializable]
    public class InvalidSymbolSizeException : System.Exception
    {
        private static string baseMessage = "Invalid symbol size.";

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidSymbolSizeException class.
        /// </summary>
        public InvalidSymbolSizeException()
            : base(baseMessage)
        { }

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.InvalidSymbolSizeException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
        public InvalidSymbolSizeException(string message)
            : base(baseMessage + Environment.NewLine + message)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidSymbolSizeException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
        public InvalidSymbolSizeException(string message, Exception innerException)
            : base(baseMessage + Environment.NewLine + message, innerException)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidSymbolSizeException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected InvalidSymbolSizeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// ZintNet.InvalidDataLengthException class.
    /// </summary>
	[Serializable]
	public class InvalidDataLengthException : System.Exception
	{
        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataLengthException class.
        /// </summary>
	    public InvalidDataLengthException()
			: base()
		{}

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataLengthException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
	    public InvalidDataLengthException( string message ) 
			: base( message )
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataLengthException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
		public InvalidDataLengthException( string message, Exception innerException ) 
			: base( message, innerException )
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataLengthException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
	    protected InvalidDataLengthException( SerializationInfo info, StreamingContext context ) :
			 base( info, context )
		{}
	}

    /// <summary>
    /// ZintNet.InvalidDataFormatException class.
    /// </summary>
	[Serializable]
	public class InvalidDataFormatException : System.Exception
	{
	    private	static string baseMessage = "Data format error in the barcode message.";

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataFormatException class.
        /// </summary>
	    public InvalidDataFormatException()
			: base( baseMessage )
		{}

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataFormatException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
		public InvalidDataFormatException( string message ) 
			: base( baseMessage + Environment.NewLine + message )
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataFormatException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
		public InvalidDataFormatException( string message, Exception innerException ) 
			: base( baseMessage + Environment.NewLine + message, innerException )
		{}

        /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataFormatException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
	    protected InvalidDataFormatException( SerializationInfo info, StreamingContext context )
            : base( info, context )
		{}
	}

    /// <summary>
    /// ZintNet.ErrorCorrectionLevelException class.
    /// </summary>
    [Serializable]
    public class ErrorCorrectionLevelException : System.Exception
    {
        private static string mbaseMessage = "Error correction level not supported.";

        /// <summary>
        /// Initialses a new instance of ZintNet.ErrorCorrectionLevelException class.
        /// </summary>
        public ErrorCorrectionLevelException()
            : base(mbaseMessage)
        { }

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.InvalidDataFormatException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
        public ErrorCorrectionLevelException(string message)
            : base(mbaseMessage + Environment.NewLine + message)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.ErrorCorrectionLevelException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
        public ErrorCorrectionLevelException(string message, Exception innerException)
            : base(mbaseMessage + Environment.NewLine + message, innerException)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.ErrorCorrectionLevelException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected ErrorCorrectionLevelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// ZintNet.DataEncodingException class.
    /// </summary>
    [Serializable]
    public class DataEncodingException : System.Exception
    {
        private static string baseMessage = "Error encoding barcode data.";

        /// <summary>
        /// Initialses a new instance of ZintNet.DataEncodingException class.
        /// </summary>
        public DataEncodingException()
            : base(baseMessage)
        { }

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.DataEncodingException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
        public DataEncodingException(string message)
            : base(baseMessage + Environment.NewLine + message)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.DataEncodingException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
        public DataEncodingException(string message, Exception innerException)
            : base(baseMessage + Environment.NewLine + message, innerException)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.DataEncodingException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected DataEncodingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// ZintNet.UnknownSymbolException class.
    /// </summary>
    [Serializable]
    public class UnknownSymbolException : System.Exception
    {
        private static string baseMessage = "Unsupported or unknown symbol: ";

        /// <summary>
        /// Initialses a new instance of ZintNet.UnknownSymbolException class.
        /// </summary>
        public UnknownSymbolException()
            : base(baseMessage)
        { }

        /// <summary>
        /// /// <summary>
        /// Initialses a new instance of ZintNet.UnknownSymbolException class with a specified error message.
        /// </summary>
        /// <param name="message">exception message</param>
        /// </summary>
        public UnknownSymbolException(string message)
            : base(baseMessage + message)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.UnknownSymbolException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="innerException">inner exception</param>
        public UnknownSymbolException(string message, Exception innerException)
            : base(baseMessage + Environment.NewLine + message, innerException)
        { }

        /// <summary>
        /// Initialses a new instance of ZintNet.UnknownSymbolException class with a specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected UnknownSymbolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}