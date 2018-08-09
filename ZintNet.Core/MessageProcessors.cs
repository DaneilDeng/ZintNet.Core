/* MessageProcessors.cs - Pre processors for the barcode data to be encoded */

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
using System.Globalization;
using System.Text;

namespace ZintNet
{
    /// <summary>
    /// Performs preprocessing checks of the barcode data.
    /// </summary>
    internal static class MessagePreProcessor
    {
        /// <summary>
        /// Check that the message format conforms with the HIBC stardard
        /// </summary>
        /// <param name="message">input message</param>
        /// <returns>character array holding the message</returns>

        public static char[] HIBCParser(string message)
        {
            if (message.Length > 110)
                throw new InvalidDataLengthException("HIBC: Input data too long.");

            foreach (char c in message)
            {
                if (CharacterSets.Code39Set.IndexOf(c) == -1)
                    throw new InvalidDataException("HIBC: Invalid data in input.");
            }

            // First character must be a HIBC Supplier Labeling flag.
            if (message[0] != '+')
                message = "+" + message;

            return message.ToCharArray();
        }

        /// <summary>
        /// Checks for valid characters and the correct formation of the AI's.
        /// </summary>
        /// <param name="message">input message</param>
        /// <returns>character array holding the message</returns>

        public static char[] AIParser(string message)
        {
            char[] barcodeData;
            StringBuilder bcData = new StringBuilder();
            bool invalidData;
            string aiString;
            int[] aiValue = new int[100];
            int[] aiLocation = new int[100];
            int[] dataLocation = new int[100];
            int[] dataLength = new int[100];
            int inputLength = message.Length;

            // Detect extended ASCII characters.
            for (int i = 0; i < inputLength; i++)
            {
                if (message[i] >= 128)
                    throw new InvalidDataException("GS1: Extended ASCII characters are not supported.");

                if (message[i] < 32)
                    throw new InvalidDataException("GS1: Control characters are not supported.");
            }

            // GS1 must start with an AI.
            if (message[0] != '[')
                throw new InvalidDataFormatException("GS1: Format must start with an Application Identifier.");

            int bracketLevel = 0;
            int maxBracketLevel = 0;
            int aiLength = 0;
            int maxAILength = 0;
            int minAILength = 5;
            int j = 0;
            invalidData = false;
            int errorValue = 0;

            // Check the bracket formatting and inputLength of AI's.
            for (int i = 0; i < inputLength; i++)
            {
                aiLength += j;
                if (j == 1 && message[i] != ']' && !Char.IsDigit(message[i]))
                    invalidData = true;

                if (message[i] == '[')
                {
                    bracketLevel++;
                    j = 1;
                }

                if (message[i] == ']')
                {
                    bracketLevel--;
                    if (aiLength < minAILength)
                        minAILength = aiLength;

                    j = 0;
                    aiLength = 0;
                }

                if (bracketLevel > maxBracketLevel)
                    maxBracketLevel = bracketLevel;

                if (aiLength > maxAILength)
                    maxAILength = aiLength;
            }

            minAILength--;

            // Check for invalid data or malformed AI's.
            if (bracketLevel != 0 || maxBracketLevel > 1)	// AI brackets not formatted correctly.
                throw new InvalidDataFormatException("GS1: Invalid Application Identifier formatting.");

            if (maxAILength > 4 || minAILength <= 1)	// AI is too long or too short.
                throw new InvalidDataFormatException("GS1: Invalid length for an Application Identifier.");

            if (invalidData == true)	// Non-numeric data in AI.
                throw new InvalidDataFormatException("GS1: Non-numeric data in Application Identifier.");

            // Find the number of AI's in the message.
            int aiCount = 0;
            for (int i = 1; i < inputLength; i++)
            {
                if (message[i - 1] == '[')
                {
                    aiString = String.Empty;
                    aiLocation[aiCount] = i;
                    j = 0;
                    do
                    {
                        aiString += message[i + j];
                        j++;
                    }
                    while (aiString[j - 1] != ']');

                    aiString = aiString.Substring(0, j - 1);
                    aiValue[aiCount] = int.Parse(aiString, CultureInfo.CurrentCulture);
                    aiCount++;
                }
            }

            for (int i = 0; i < aiCount; i++)
            {
                dataLocation[i] = aiLocation[i] + 3;
                if (aiValue[i] >= 100)
                    dataLocation[i]++;

                if (aiValue[i] >= 1000)
                    dataLocation[i]++;

                dataLength[i] = 0;
                do
                    dataLength[i]++;
                while ((dataLocation[i] + dataLength[i] - 1) < inputLength && message[dataLocation[i] + dataLength[i] - 1] != '[');
                dataLength[i]--;
            }

            for (int i = 0; i < aiCount; i++)
            {
                if (dataLength[i] == 0)	// No data for given AI.
                    throw new InvalidDataFormatException("GS1: No data field for AI.");
            }

            aiString = String.Empty;
            for (int i = 0; i < aiCount; i++)
            {
                errorValue = 2;
                switch (aiValue[i])
                {
                    // Length 2 Fixed
                    case 20: // VARIANT
                        errorValue = 0;
                        if (dataLength[i] != 2)
                            errorValue = 1;

                        break;
                    // Length 3 Fixed
                    case 422: // ORIGIN
                    case 424: // COUNTRY PROCESS
                    case 426: // COUNTRY FULL PROCESS
                        errorValue = 0;
                        if (dataLength[i] != 3)
                            errorValue = 1;

                        break;
                    // Length 4 Fixed
                    case 8111: // POINTS
                        errorValue = 0;
                        if (dataLength[i] != 4)
                            errorValue = 1;

                        break;
                    // Length 6 Fixed
                    case 11: // PROD DATE
                    case 12: // DUE DATE
                    case 13: // PACK DATE
                    case 15: // BEST BY
                    case 16: // SELL BY
                    case 17: // USE BY
                    case 7006: // FIRST FREEZE DATE
                    case 8005: // PRICE PER UNIT
                        errorValue = 0;
                        if (dataLength[i] != 6)
                            errorValue = 1;

                        break;
                    // Length 10 Fixed
                    case 7003: // EXPIRY TIME
                        errorValue = 0;
                        if (dataLength[i] != 10)
                            errorValue = 1;

                        break;
                    // Length 13 Fixed
                    case 410:   // SHIP TO LOC
                    case 411:   // BILL TO
                    case 412:   // PURCHASE FROM
                    case 413:   // SHIP FOR LOC
                    case 414:   // LOC NO
                    case 415:   // PAY TO
                    case 416:   // PROD/SERV LOC
                    case 7001:  // NSN
                        errorValue = 0;
                        if (dataLength[i] != 13)
                            errorValue = 1;

                        break;
                    // Length 14 Fixed
                    case 1:     // GTIN
                    case 2:     // CONTENT
                    case 8001:  // DIMENSIONS
                        errorValue = 0;
                        if (dataLength[i] != 14)
                            errorValue = 1;

                        break;
                    // Length 17 Fixed.
                    case 402:   // GSIN
                    case 8017:  // GSRN PROVIDER
                    case 8018:  // GSRN RECIPIENT
                        errorValue = 0;
                        if (dataLength[i] != 17)
                            errorValue = 1;

                        break;
                    // Length 18 Fixed.
                    case 0:     // SSCC
                    case 8006:  // ITIP
                        errorValue = 0;
                        if (dataLength[i] > 18)
                            errorValue = 1;

                        break;
                    // Length 2 Maximum.
                    case 7010:
                        errorValue = 0;
                        if (dataLength[i] > 2)
                            errorValue = 1;

                        break;

                    // Length 3 Maximum.
                    case 427:   // ORIGIN SUBDIVISION
                    case 7008:  // AQUATIC SPECIES
                        errorValue = 0;
                        if(dataLength[i] > 3)
                            errorValue = 1;

                        break;
                    // Length 4 Maximum.
                    case 7004:  // ACTIVE POTENCY
                        errorValue = 0;
                        if (dataLength[i] > 4)
                            errorValue = 1;

                        break;
                    // Length 6 Maximum
                    case 242: // MTO VARIANT
                        errorValue = 0;
                        if (dataLength[i] > 6)
                            errorValue = 1;

                        break;
                    // Length 8 Max
                    case 30: // VAR COUNT
                    case 37: // COUNT
                        errorValue = 0;
                        if (dataLength[i] > 8)
                            errorValue = 1;

                        break;
                    // Length 10 Maximum.
                    case 7009: // FISHING GEAR TYPE
                    case 8019: // SRIN
                        errorValue = 0;
                        if (dataLength[i] > 10)
                            errorValue = 1;

                        break;
                    // Length 12 Maximum.
                    case 7005: // CATCH AREA
                    case 8011: // CPID SERIAL
                        errorValue = 0;
                        if (dataLength[i] > 12)
                            errorValue = 1;

                        break;
                    // Length 20 Maximum.
                    case 10: // BATCH/LOT
                    case 21: // SERIAL
                    case 22: // CPV
                    case 243: // PCN
                    case 254: // GLN EXTENSION COMPONENT
                    case 420: // SHIP TO POST
                    case 7020: // REFURB LOT
                    case 7021: // FUNC STAT
                    case 7022: // REV STAT
                    case 710: // NHRN PZN
                    case 711: // NHRN CIP
                    case 712: // NHRN CN
                    case 713: // NHRN DRN
                    case 714: // NHRN AIM
                    case 8002: // CMT NO
                    case 8012: // VERSION
                        errorValue = 0;
                        if (dataLength[i] > 20)
                            errorValue = 1;

                        break;
                    // Length 25 Max
                    case 8020: // REF NO
                        errorValue = 0;
                        if (dataLength[i] > 25)
                            errorValue = 1;

                        break;
                    // Length 30 Max
                    case 240:   // ADDITIONAL ID
                    case 241:   // CUST PART NO
                    case 250:   // SECONDARY SERIAL
                    case 251:   // REF TO SOURCE
                    case 400:   // ORDER NUMBER
                    case 401:   // GINC
                    case 403:   // ROUTE
                    case 7002:  // MEAT CUT
                    case 7023:  // GIAI ASSEMBLY
                    case 8004:  // GIAI
                    case 8010:  // CPID
                    case 8013:  // BUDI-DI
                    case 90:    // INTERNAL
                        errorValue = 0;
                        if (dataLength[i] > 30)
                            errorValue = 1;

                        break;
                    // Length 34 Maximum
                    case 8007: // IBAN
                        errorValue = 0;
                        if (dataLength[i] > 34)
                            errorValue = 1;

                        break;
                    // Length 70 Maximum.
                    case 8110: // Coupon code
                    case 8112: // Paperless coupon code
                    case 8200: // PRODUCT URL
                        errorValue = 0;
                        if (dataLength[i] > 34)
                            errorValue = 1;

                        break;
                }

                if (aiValue[i] == 253)  // GDTI
                {
                    errorValue = 0;
                    if (dataLength[i] > 14 || dataLength[i] > 31)
                        errorValue = 1;
                }

                if (aiValue[i] == 254)  // GCN
                {
                    errorValue = 0;
                    if (dataLength[i] < 14 || dataLength[i] > 25)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3100 && aiValue[i] <= 3169)
                {
                    errorValue = 0;
                    if (dataLength[i] != 6)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3200 && aiValue[i] <= 3379)
                {
                    errorValue = 0;
                    if (dataLength[i] != 6)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3400 && aiValue[i] <= 3579)
                {
                    errorValue = 0;
                    if (dataLength[i] != 6)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3600 && aiValue[i] <= 3699)
                {
                    errorValue = 0;
                    if (dataLength[i] != 6)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3900 && aiValue[i] <= 3909)   // AMOUNT
                {
                    errorValue = 0;
                    if (dataLength[i] > 15)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3910 && aiValue[i] <= 3919)   // AMOUNT
                {
                    errorValue = 0;
                    if (dataLength[i] > 15)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3920 && aiValue[i] <= 3929)   // PRICE
                {
                    errorValue = 0;
                    if (dataLength[i] > 15)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3930 && aiValue[i] <= 3939)   // PRICE
                {
                    errorValue = 0;
                    if (dataLength[i] < 4 || dataLength[i] > 18)
                        errorValue = 1;
                }

                if (aiValue[i] >= 3940 && aiValue[i] <= 3949)   // PRCNT OFF
                {
                    errorValue = 0;
                    if (dataLength[i] != 4)
                        errorValue = 1;
                }

                if (aiValue[i] == 421)  // SHIP TO POST
                {
                    errorValue = 0;
                    if (dataLength[i] < 4 || dataLength[i] > 12)
                        errorValue = 1;
                }

                if ((aiValue[i] == 423) || (aiValue[i] == 425)) // COUNTRY INITIAL PROCESS || COUNTRY DISASSEMBLY
                {
                    errorValue = 0;
                    if ((dataLength[i] < 4) || (dataLength[i] > 15))
                        errorValue = 1;
                }

                if (aiValue[i] == 7007) // HARVEST DATE
                {
                    errorValue = 0;
                    if ((dataLength[i] < 6) || (dataLength[i] > 12))
                        errorValue = 1;
                }

                if ((aiValue[i] >= 7030) && (aiValue[i] <= 7039))   // PROCESSOR #
                {
                    errorValue = 0;
                    if ((dataLength[i] < 4) || (dataLength[i] > 30))
                        errorValue = 1;
                }

                if (aiValue[i] == 8003) // GRAI
                {
                    errorValue = 0;
                    if ((dataLength[i] < 15) || (dataLength[i] > 30))
                        errorValue = 1;
                }

                if (aiValue[i] == 8008) // PROD TIME
                {
                    errorValue = 0;
                    if ((dataLength[i] < 9) || (dataLength[i] > 12))
                        errorValue = 1;
                }

                if ((aiValue[i] >= 91) && (aiValue[i] <= 99))   // INTERNAL
                {
                    errorValue = 0;
                    if (dataLength[i] > 90)
                        errorValue = 1;
                }

                if (errorValue > 0)
                    aiString = String.Format(CultureInfo.CurrentCulture, "{0:d2}", aiValue[i]);     // Error has just been detected: capture AI.

                if (errorValue == 1)
                    throw new InvalidDataFormatException("GS1: Invalid data length for AI [" + aiString + "].");

                if (errorValue == 2)
                    throw new InvalidDataFormatException("GS1: Invalid AI value [" + aiString + "].");
            }

            // Resolve AI data.
            int lastAI = 0;
            bool aiLatch = true;

            for (int i = 0; i < inputLength; i++)
            {
                if (message[i] != '[' && message[i] != ']')
                    bcData.Append(message[i]);

                if (message[i] == '[')
                {
                    // Start of an AI string.
                    if (aiLatch == false)
                        bcData.Append("[");

                    aiString = message.Substring(i + 1, 2);
                    lastAI = int.Parse(aiString, CultureInfo.CurrentCulture);
                    aiLatch = false;
                    // The following values from GS-1 General Specification version 8.0 issue 2, May 2008
                    // figure 5.4.8.2.1 - 1 Element Strings with Pre-Defined Length Using Application Identifiers.
                    if ((lastAI >= 0 && lastAI <= 4)
                        || (lastAI >= 11 && lastAI <= 20)
                        || lastAI == 23	 // Legacy support - see 5.3.8.2.2.
                        || (lastAI >= 31 && lastAI <= 36)
                        || lastAI == 41)
                        aiLatch = true;
                }
                // The ']' character is simply dropped from the input.
            }

            // The character '[' in the returned string refers to the FNC1 character.
            return barcodeData = bcData.ToString().ToCharArray();
        }


        /// <summary>
        /// Parse the barcode message and process any 'tilde' values.
        /// </summary>
        /// <remarks>
        /// Takes the message string and copies it to the barcode character array, processing any 'tilde' values.
        /// 1 digit decimal values ~@ to ~'
        /// Or 3 digit decimal values from ~000 to ~255
        /// Suitable for symbols that accept values 0 to 255
        /// </remarks>
        /// <param name="message">barcode message string</param>
        /// <returns>character array holding the message</returns>
        public static char[] TildeParser(string message)
        {
            int inputLength = message.Length;
            StringBuilder bcData = new StringBuilder();

            for (int i = 0; i < inputLength; i++)
            {
                // Has a tilde command fllowed by a single character in the range @ to '.
                // eg. ~M
                if (message[i] == '~' && i != (inputLength - 1))
                {
                    // Look ahead one character and test it's in the range "@ to '".
                    // If it is, translate to decimal "000 to 031".
                    int nextChar = (int)(message[i + 1]);
                    if (nextChar > 63 && nextChar < 97)
                    {
                        bcData.Append((char)(nextChar - 64));
                        i++;
                        continue;
                    }

                    // Maybe a 3 character tilde command.
                    else if (i < (inputLength - 3))
                    {
                        // Look ahead 3 characters, try to translate to an integer value.
                        // If sucessful add the ascii value to the message array.
                        // Else treat each character as individual values.
                        int asciiValue;
                        string asciiString = message.Substring(i + 1, 3);
                        if (int.TryParse(asciiString, out asciiValue))
                        {
                            if (asciiValue < 256)
                            {
                                bcData.Append((char)asciiValue);
                                i += 3;
                            }

                            else
                                bcData.Append(message[i]); ;
                        }

                        continue;
                    }
                }

                bcData.Append(message[i]);
            }

            return bcData.ToString().ToCharArray();
        }

        /// <summary>
        /// Returns the message as a character array without any preprocessing or checks.
        /// </summary>
        /// <param name="message">input message</param>
        /// <returns>character array holding the message</returns>
        public static char[] MessageParser(string message)
        {
            return message.ToCharArray();
        }

        /// <summary>
        /// Passes an input message of numeric values only.
        /// </summary>
        /// <param name="message">input message</param>
        /// <returns>character array holding the message</returns>
        public static char[] NumericOnlyParser(string message)
        {
            foreach (char c in message)
            {
                if (!char.IsDigit(c))
                    throw new InvalidDataException("Numeric only data expected.");
            }

            return message.ToCharArray();
        }
    }
}