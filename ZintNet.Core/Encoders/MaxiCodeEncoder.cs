/* MaxiCodeEncoder.cs - Handles MaxiCode 2D symbol */

/*
    ZintNetLib - a C# port of libzint.
    Copyright (C) 2013-2017 Milton Neal <milton200954@gmail.com>
    Acknowledgments to Robin Stuart and other Zint Authors and Contributors.
 
    libzint - the open source barcode library
    Copyright (C) 2008-2016 Robin Stuart <rstuart114@gmail.com>

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
using System.Collections.ObjectModel;
using System.Globalization;

namespace ZintNet.Core.Encoders
{
    internal class MaxiCodeEncoder : SymbolEncoder
    {
        #region Tables
        // ISO/IEC 16023 Figure 5 - MaxiCode Module Sequence 30 x 33 data grid.
        static int[] MaxiGrid = {
		    122, 121, 128, 127, 134, 133, 140, 139, 146, 145, 152, 151, 158, 157, 164, 163, 170, 169,
		    176, 175, 182, 181, 188, 187, 194, 193, 200, 199, 0, 0,	124, 123, 130, 129, 136, 135, 142,
			141, 148, 147, 154, 153, 160, 159, 166, 165, 172, 171, 178, 177, 184, 183, 190, 189, 196,
			195, 202, 201, 817, 0, 126, 125, 132, 131, 138, 137, 144, 143, 150, 149, 156, 155, 162, 161,
			168, 167, 174, 173, 180, 179, 186, 185, 192, 191, 198, 197, 204, 203, 819, 818,	284, 283, 278,
			277, 272, 271, 266, 265, 260, 259, 254, 253, 248, 247, 242, 241, 236, 235, 230, 229, 224, 223,
			218, 217, 212, 211, 206, 205, 820, 0, 286, 285, 280, 279, 274, 273, 268, 267, 262, 261, 256,
			255, 250, 249, 244, 243, 238, 237, 232, 231, 226, 225, 220, 219, 214, 213, 208, 207, 822, 821,
			288, 287, 282, 281, 276, 275, 270, 269, 264, 263, 258, 257, 252, 251, 246, 245, 240, 239, 234,
			233, 228, 227, 222, 221, 216, 215, 210, 209, 823, 0, 290, 289, 296, 295, 302, 301, 308, 307, 314,
			313, 320, 319, 326, 325, 332, 331, 338, 337, 344, 343, 350, 349, 356, 355, 362, 361, 368, 367, 
			825, 824, 292, 291, 298, 297, 304, 303, 310, 309, 316, 315, 322, 321, 328, 327, 334, 333, 340,
			339, 346, 345, 352, 351, 358, 357, 364, 363, 370, 369, 826, 0, 294, 293, 300, 299, 306, 305, 312,
			311, 318, 317, 324, 323, 330, 329, 336, 335, 342, 341, 348, 347, 354, 353, 360, 359, 366, 365,
			372, 371, 828, 827,	410, 409, 404, 403, 398, 397, 392, 391, 80, 79, 0, 0, 14, 13, 38, 37, 3, 0,
			45, 44, 110, 109, 386, 385, 380, 379, 374, 373, 829, 0,	412, 411, 406, 405, 400, 399, 394, 393,
			82, 81, 41, 0, 16, 15, 40, 39, 4, 0, 0, 46, 112, 111, 388, 387, 382, 381, 376, 375, 831, 830,
			414, 413, 408, 407, 402, 401, 396, 395, 84, 83, 42, 0, 0, 0, 0, 0, 6, 5, 48, 47, 114, 113, 390,
			389, 384, 383, 378, 377, 832, 0, 416, 415, 422, 421, 428, 427, 104, 103, 56, 55, 17, 0, 0, 0, 0,
			0, 0, 0, 21, 20, 86, 85, 434, 433, 440, 439, 446, 445, 834, 833, 418, 417, 424, 423, 430, 429,
			106, 105, 58, 57, 0, 0, 0, 0, 0, 0, 0, 0, 23, 22, 88, 87, 436, 435, 442, 441, 448, 447, 835, 0,
			420, 419, 426, 425, 432, 431, 108, 107, 60, 59, 0, 0, 0, 0, 0, 0, 0, 0, 0, 24, 90, 89, 438, 437,
			444, 443, 450, 449, 837, 836, 482, 481, 476, 475, 470, 469, 49, 0, 31, 0, 0, 0, 0, 0, 0, 0, 0, 
			0, 0, 1, 54, 53, 464, 463, 458, 457, 452, 451, 838, 0, 484, 483, 478, 477, 472, 471, 50, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 466, 465, 460, 459, 454, 453, 840, 839, 486, 485, 480,
			479, 474, 473, 52, 51, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 43, 468, 467, 462, 461, 456, 455,
			841, 0,	488, 487, 494, 493, 500, 499, 98, 97, 62, 61, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 92, 91,
			506, 505, 512, 511, 518, 517, 843, 842,	490, 489, 496, 495, 502, 501, 100, 99, 64, 63, 0, 0, 0,
			0, 0, 0, 0, 0, 29, 28, 94, 93, 508, 507, 514, 513, 520, 519, 844, 0, 492, 491, 498, 497, 504,
			503, 102, 101, 66, 65, 18, 0, 0, 0, 0, 0, 0, 0, 19, 30, 96, 95, 510, 509, 516, 515, 522, 521,
			846, 845, 560, 559, 554, 553, 548, 547, 542, 541, 74, 73, 33, 0, 0, 0, 0, 0, 0, 11, 68, 67, 116,
			115, 536, 535, 530, 529, 524, 523, 847, 0, 562, 561, 556, 555, 550, 549, 544, 543, 76, 75, 0, 0,
			8, 7, 36, 35, 12, 0, 70, 69, 118, 117, 538, 537, 532, 531, 526, 525, 849, 848, 564, 563, 558,
			557, 552, 551, 546, 545, 78, 77, 0, 34, 10, 9, 26, 25, 0, 0, 72, 71, 120, 119, 540, 539, 534,
			533, 528, 527, 850, 0, 566, 565, 572, 571, 578, 577, 584, 583, 590, 589, 596, 595, 602, 601, 608,
			607, 614, 613, 620, 619, 626, 625, 632, 631, 638, 637, 644, 643, 852, 851, 568, 567, 574, 573,
			580, 579, 586, 585, 592, 591, 598, 597, 604, 603, 610, 609, 616, 615, 622, 621, 628, 627, 634,
			633, 640, 639, 646, 645, 853, 0, 570, 569, 576, 575, 582, 581, 588, 587, 594, 593, 600, 599, 606,
			605, 612, 611, 618, 617, 624, 623, 630, 629, 636, 635, 642, 641, 648, 647, 855, 854, 728, 727,
			722, 721, 716, 715, 710, 709, 704, 703, 698, 697, 692, 691, 686, 685, 680, 679, 674, 673, 668,
			667, 662, 661, 656, 655, 650, 649, 856, 0, 730, 729, 724, 723, 718, 717, 712, 711, 706, 705, 700,
			699, 694, 693, 688, 687, 682, 681, 676, 675, 670, 669, 664, 663, 658, 657, 652, 651, 858, 857,
			732, 731, 726, 725, 720, 719, 714, 713, 708, 707, 702, 701, 696, 695, 690, 689, 684, 683, 678,
			677, 672, 671, 666, 665, 660, 659, 654, 653, 859, 0, 734, 733, 740, 739, 746, 745, 752, 751, 758,
			757, 764, 763, 770, 769, 776, 775, 782, 781, 788, 787, 794, 793, 800, 799, 806, 805, 812, 811,
			861, 860, 736, 735, 742, 741, 748, 747, 754, 753, 760, 759, 766, 765, 772, 771, 778, 777, 784,
			783, 790, 789, 796, 795, 802, 801, 808, 807, 814, 813, 862, 0, 738, 737, 744, 743, 750, 749, 756,
			755, 762, 761, 768, 767, 774, 773, 780, 779, 786, 785, 792, 791, 798, 797, 804, 803, 810, 809,
			816, 815, 864, 863};

        // From Appendix A - ASCII character to Code Set (e.g. 2 = Set B)
        // Set 0 refers to special characters that fit into more than one set (e.g. GS)
        static int[] MaxiCodeSet = {
			 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 0, 5, 5, 5, 5, 5, 5,
			 5, 5, 5, 5, 5, 5, 5, 5, 0, 0, 0, 5, 0, 2, 1, 1, 1, 1, 1, 1,
			 1, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 2,
			 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4,
			 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			 5, 4, 5, 5, 5, 5, 5, 5, 4, 5, 3, 4, 3, 5, 5, 4, 4, 3, 3, 3,
			 4, 3, 5, 4, 4, 3, 3, 4, 3, 3, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3,
			 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
			 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4};

        // From Appendix A - ASCII character to symbol value.
        static int[] MaxiSymbolCharacter = {
			 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			 20, 21, 22, 23, 24, 25, 26, 30, 28, 29, 30, 35, 32, 53, 34, 35, 36, 37, 38, 39,
			 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 37,
			 38, 39, 40, 41, 52, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 42, 43, 44, 45, 46, 0, 1, 2, 3,
			 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
			 24, 25, 26, 32, 54, 34, 35, 36, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 47, 48,
			 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 36,
			 37, 37, 38, 39, 40, 41, 42, 43, 38, 44, 37, 39, 38, 45, 46, 40, 41, 39, 40, 41,
			 42, 42, 47, 43, 44, 43, 44, 45, 45, 46, 47, 46, 0, 1, 2, 3, 4, 5, 6, 7,
			 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 32,
			 33, 34, 35, 36, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 32, 33, 34, 35, 36};

        #endregion

        MaxicodeMode mode;

        public MaxiCodeEncoder(Symbology symbology, string barcodeMessage, MaxicodeMode mode, int eci)
        {
            this.symbolId = symbology;
            this.barcodeMessage = barcodeMessage;
            this.mode = mode;
            this.eci = eci;
        }

        public override Collection<SymbolData> EncodeData()
        {
            Symbol = new Collection<SymbolData>();
            barcodeData = MessagePreProcessor.TildeParser(barcodeMessage);
            MaxiCode();
            return Symbol;
        }

        private void MaxiCode()
        {
            int inputLength = barcodeData.Length;
            int eccLength;
            int[] bitPattern = new int[7];
            int countryCode = 0;
            int serviceClass = 0;
            char[] postcode;
            char[] primaryData;
            char[] secondaryData = new char[inputLength];
            int[] maxiCodewords = new int[144];
            int[,] symbolGrid = new int[33, 30];
            char[] scmHeader = { '[', ')', '>', (char)(0x1e), '0', '1', (char)(0x1d) };

            if (mode == MaxicodeMode.Mode2 || mode == MaxicodeMode.Mode3)
            {
                // Process the primary data.
                // Check for valid structured carrier message header.
                for (int i = 0; i < scmHeader.Length; i++ )
                {
                    if(barcodeData[i] != scmHeader[i])
                        throw new InvalidDataFormatException("Maxicode: Invalid Mode 2 / Mode 3 structured carrier message header.");
                }

                // Check for mandatory UPS data requirements.
                // Header + Postcode + Country Code + Service Type + Tracking Code + SCAC + EOM (44 characters mode 2, 41 for mode 3).
                int minimumDataLength = (mode == MaxicodeMode.Mode2) ? 44 : 41;
                if (inputLength < minimumDataLength)
                    throw new InvalidDataException("Maxicode: Input data is less than mandatory UPS requirements.");

                // Check for the EOM.
                if (Array.IndexOf(barcodeData, (char)(0x04)) == -1)
                    throw new InvalidDataFormatException("Maxicode: EOM (0x04 Hex) marker missing in input data.");
                // Extract the postcode, country code and service class and save in the primary data buffer.
                // Note: Group separators (GS = decimal 029) are removed.
                int primaryDataLength = (mode == MaxicodeMode.Mode2) ? 15 : 12;
                primaryData = new char[primaryDataLength];

                int offset = 9;     // Start of postcode.
                int idx = 9;
                int postcodeLength = 0;
                do
                {
                    postcodeLength++;
                    idx++;
                } while (barcodeData[idx] != (char)(0x1d) && idx < inputLength);

                if (idx == inputLength)
                    throw new InvalidDataFormatException("Maxicode: Input data missing GS.");

                if ((mode == MaxicodeMode.Mode2 && postcodeLength != 9) || (mode == MaxicodeMode.Mode3 && postcodeLength != 6))
                    throw new InvalidDataException("Maxicode: Invalid postcode in input data.");

                Array.Copy(barcodeData, offset, primaryData, 0, postcodeLength);
                idx++;
                offset = idx;
                int countryCodeLength = 0;
                do
                {
                    countryCodeLength++;
                    idx++;
                } while (barcodeData[idx] != (char)(0x1d) && idx < inputLength);

                if (idx == inputLength)
                    throw new InvalidDataFormatException("Input data missing GS.");

                if (countryCodeLength != 3)
                    throw new InvalidDataLengthException("Maxicode: Invalid length for Country Code");

                Array.Copy(barcodeData, offset, primaryData, postcodeLength, countryCodeLength);
                idx++;
                offset = idx;
                int serviceClassLength = 0;
                do
                {
                    serviceClassLength++;
                    idx++;
                } while (barcodeData[idx] != (char)(0x1d) && idx < inputLength);

                if (idx == inputLength)
                    throw new InvalidDataFormatException("Maxicode: Input data missing GS.");

                if (serviceClassLength != 3)
                    throw new InvalidDataLengthException("Maxicode: Invalid length for Service Class");

                Array.Copy(barcodeData, offset, primaryData, postcodeLength + countryCodeLength, serviceClassLength);
                idx++;
                offset = idx;
                if (mode == MaxicodeMode.Mode2)
                {
                    for (int i = 0; i < primaryData.Length; i++ )
                    {
                        if(!Char.IsDigit(primaryData[i]))
                            throw new InvalidDataException("Maxicode: Primary data must be numeric.");
                    }

                    postcode = new char[9];
                    Array.Copy(primaryData, postcode, 9);
                    countryCode = int.Parse(new string(primaryData, 9, 3), CultureInfo.CurrentCulture);
                    if (countryCode != 840)
                        throw new InvalidDataException("Maxicode: Mode 2 requires US Country Code 840.");

                    serviceClass = int.Parse(new string(primaryData, 12, 3), CultureInfo.CurrentCulture);
                    PrimaryMode2(postcode, countryCode, serviceClass, maxiCodewords);
                }

                else
                {
                    postcode = new char[6];
                    Array.Copy(primaryData, postcode, 6);
                    ArrayExtensions.ToUpper(primaryData);
                    if (!IsNumeric(primaryData, 6, 6))
                        throw new InvalidDataException("Maxicode: Primary data must be numeric.");

                    countryCode = int.Parse(new string(primaryData, 6, 3), CultureInfo.CurrentCulture);
                    serviceClass = int.Parse(new string(primaryData, 9, 3), CultureInfo.CurrentCulture);
                    PrimaryMode3(postcode, countryCode, serviceClass, maxiCodewords);
                }

                // Copy the header and the 2 digit year into the secondary data buffer.
                Array.Copy(barcodeData, 0, secondaryData, 0, 9);
                // Copy the rest of the data into the secondary data buffer.
                Array.Copy(barcodeData, offset, secondaryData, 9, inputLength - offset);
                int secondaryLength = Array.IndexOf(secondaryData, (char)(0x04));
                Array.Resize(ref secondaryData, secondaryLength + 1);
            }

            else
            {
                // Not using a primary, just copy all the data into the secondary data buffer.
                maxiCodewords[0] = (int)mode;
                Array.Copy(barcodeData, secondaryData, inputLength);
            }

            if (!ProcessText(secondaryData, maxiCodewords))
                throw new InvalidDataLengthException("Maxicode: Input data too long.");

            // All the data is sorted - now do error correction.
            PrimaryDataCheck(maxiCodewords);     // Always EEC.

            if (mode == MaxicodeMode.Mode5)
                eccLength = 56;     // 68 data codewords, 56 error corrections

            else
                eccLength = 40;     // 84 data codewords, 40 error corrections

            SecondaryDataCheckEven(eccLength / 2, maxiCodewords);  // Do error correction of even.
            SecondaryDataCheckOdd(eccLength / 2, maxiCodewords);   // Do error correction of odd.

            // Copy data into symbol grid.
            for (int row = 0; row < 33; row++)
            {
                for (int column = 0; column < 30; column++)
                {
                    int block = (MaxiGrid[(row * 30) + column] + 5) / 6;
                    int bit = (MaxiGrid[(row * 30) + column] + 5) % 6;
                    if (block != 0)
                    {
                        bitPattern[0] = (maxiCodewords[block - 1] & 0x20) >> 5;
                        bitPattern[1] = (maxiCodewords[block - 1] & 0x10) >> 4;
                        bitPattern[2] = (maxiCodewords[block - 1] & 0x8) >> 3;
                        bitPattern[3] = (maxiCodewords[block - 1] & 0x4) >> 2;
                        bitPattern[4] = (maxiCodewords[block - 1] & 0x2) >> 1;
                        bitPattern[5] = (maxiCodewords[block - 1] & 0x1);

                        if (bitPattern[bit] != 0)
                            SetModule(row, column, symbolGrid);
                    }
                }
            }

            // Add orientation markings.
            SetModule(0, 28, symbolGrid);	// Top right filler
            SetModule(0, 29, symbolGrid);
            SetModule(9, 10, symbolGrid);	// Top left marker
            SetModule(9, 11, symbolGrid);
            SetModule(10, 11, symbolGrid);
            SetModule(15, 7, symbolGrid);	// Left hand marker
            SetModule(16, 8, symbolGrid);
            SetModule(16, 20, symbolGrid);	// Right hand marker
            SetModule(17, 20, symbolGrid);
            SetModule(22, 10, symbolGrid);	// Bottom left marker
            SetModule(23, 10, symbolGrid);
            SetModule(22, 17, symbolGrid);	// Bottom right marker
            SetModule(23, 17, symbolGrid);

            // Expand the row pattern into the symbol data.
            byte[] rowData;
            for (int y = 0; y < 33; y++)
            {
                rowData = new byte[30];
                for (int x = 0; x < 30; x++)
                    rowData[x] = GetModule(y, x, symbolGrid);

                SymbolData symbolData = new SymbolData(rowData, 1.0f);
                Symbol.Add(symbolData);
            }
        }

        private bool ProcessText(char[] secondaryData, int[] maxiCodewords)
        {
            // Format text according to Appendix A.
            // This code doesn't make use of [Lock in C], [Lock in D]
            // and [Lock in E] and so is not always the most efficient at
            // compressing data, but should suffice for most applications.

            int dataLength = secondaryData.Length;
            int[] set = new int[144];
            int[] character = new int[144];
            bool done = false;
            int idx;

            if (dataLength > 138)
                return false;

            for (int i = 0; i < dataLength; i++)
            {
                // Look up characters in table from Appendix A - this gives value and code set for most characters.
                set[i] = MaxiCodeSet[secondaryData[i]];
                character[i] = MaxiSymbolCharacter[secondaryData[i]];
            }

            // If a character can be represented in more than one code set, pick which version to use.
            if (set[0] == 0)
            {
                if (character[0] == 13)
                    character[0] = 0;

                set[0] = 1;
            }

            for (int i = 1; i < dataLength; i++)
            {
                if (set[i] == 0)
                {
                    done = false;
                    // Special characters.
                    if (character[i] == 13)  // Carriage Return.
                    {
                        if (set[i - 1] == 5)
                        {
                            character[i] = 13;
                            set[i] = 5;
                        }

                        else
                        {
                            if ((i != dataLength - 1) && (set[i + 1] == 5))
                            {
                                character[i] = 13;
                                set[i] = 5;
                            }

                            else
                            {
                                character[i] = 0;
                                set[i] = 1;
                            }
                        }

                        done = true;
                    }

                    if ((character[i] == 28) && !done)   // FS.
                    {
                        if (set[i - 1] == 5)
                        {
                            character[i] = 32;
                            set[i] = 5;
                        }

                        else
                            set[i] = set[i - 1];

                        done = true;
                    }

                    if ((character[i] == 29) && !done)   // GS.
                    {
                        if (set[i - 1] == 5)
                        {
                            character[i] = 33;
                            set[i] = 5;
                        }

                        else
                            set[i] = set[i - 1];

                        done = true;
                    }

                    if ((character[i] == 30) && !done)   // RS.
                    {
                        if (set[i - 1] == 5)
                        {
                            character[i] = 34;
                            set[i] = 5;
                        }

                        else
                            set[i] = set[i - 1];

                        done = true;
                    }

                    if ((character[i] == 32) && !done)   // Space.
                    {
                        if (set[i - 1] == 1)
                        {
                            character[i] = 32;
                            set[i] = 1;
                        }

                        if (set[i - 1] == 2)
                        {
                            character[i] = 47;
                            set[i] = 2;
                        }

                        if (set[i - 1] >= 3)
                        {
                            if (i != dataLength - 1)
                            {
                                if (set[i + 1] == 1)
                                {
                                    character[i] = 32;
                                    set[i] = 1;
                                }

                                if (set[i + 1] == 2)
                                {
                                    character[i] = 47;
                                    set[i] = 2;
                                }

                                if (set[i + 1] >= 3)
                                {
                                    character[i] = 59;
                                    set[i] = set[i - 1];
                                }
                            }

                            else
                            {
                                character[i] = 59;
                                set[i] = set[i - 1];
                            }
                        }

                        done = true;
                    }

                    if ((character[i] == 44) && !done)   // Comma.
                    {
                        if (set[i - 1] == 2)
                        {
                            character[i] = 48;
                            set[i] = 2;
                        }

                        else
                        {
                            if ((i != dataLength - 1) && (set[i + 1] == 2))
                            {
                                character[i] = 48;
                                set[i] = 2;
                            }

                            else
                                set[i] = 1;
                        }

                        done = true;
                    }

                    if ((character[i] == 46) && !done)   // Full Stop.
                    {
                        if (set[i - 1] == 2)
                        {
                            character[i] = 49;
                            set[i] = 2;
                        }

                        else
                        {
                            if ((i != dataLength - 1) && (set[i + 1] == 2))
                            {
                                character[i] = 49;
                                set[i] = 2;
                            }

                            else
                                set[i] = 1;
                        }

                        done = true;
                    }

                    if ((character[i] == 47) && !done)   // Slash.
                    {
                        if (set[i - 1] == 2)
                        {
                            character[i] = 50;
                            set[i] = 2;
                        }

                        else
                        {
                            if ((i != dataLength - 1) && (set[i + 1] == 2))
                            {
                                character[i] = 50;
                                set[i] = 2;
                            }

                            else
                                set[i] = 1;
                        }

                        done = true;
                    }

                    if ((character[i] == 58) && !done)   // Colon.
                    {
                        if (set[i - 1] == 2)
                        {
                            character[i] = 51;
                            set[i] = 2;
                        }

                        else
                        {
                            if ((i != dataLength - 1) && (set[i + 1] == 2))
                            {
                                character[i] = 51;
                                set[i] = 2;
                            }

                            else
                                set[i] = 1;
                        }

                        done = true;
                    }
                }
            }

            for (int i = dataLength; i < 144; i++)
            {
                // Add the padding.
                if (set[dataLength - 1] == 2)
                    set[i] = 2;

                else
                    set[i] = 1;

                character[i] = 33;
            }

            // Find candidates for number compression.
            // Number compression not allowed on the primary data.
            // In modes 4,5 & 6 the first nine characters of the secondary data
            // are placed in the primary data.
            
            int start = (mode == MaxicodeMode.Mode2 || mode == MaxicodeMode.Mode3) ? 0 : 9;
            int count = 0;
            for (int i = start; i < 144; i++)
            {
                if ((set[i] == 1) && ((character[i] >= 48) && (character[i] <= 57)))    // Character is a number.
                    count++;

                else
                    count = 0;

                if (count == 9)
                {
                    // Nine digits in a row can be compressed.
                    set[i] = 6;
                    set[i - 1] = 6;
                    set[i - 2] = 6;
                    set[i - 3] = 6;
                    set[i - 4] = 6;
                    set[i - 5] = 6;
                    set[i - 6] = 6;
                    set[i - 7] = 6;
                    set[i - 8] = 6;
                    count = 0;
                }
            }

            // Add shift and latch characters.
            int currentSet = 1;
            idx = 0;
            do
            {
                if (set[idx] != currentSet)
                {
                    switch (set[idx])
                    {
                        case 1:
                            if (set[idx + 1] == 1)
                            {
                                if (set[idx + 2] == 1)
                                {
                                    if (set[idx + 3] == 1)
                                    {
                                        // Latch A.
                                        InsertPosition(set, character, idx);
                                        character[idx++] = 63;
                                        currentSet = 1;
                                        dataLength++;
                                    }

                                    else
                                    {
                                        // 3 Shift A.
                                        InsertPosition(set, character, idx);
                                        character[idx++] = 57;
                                        dataLength++;
                                        idx += 2;
                                    }
                                }

                                else
                                {
                                    // 2 Shift A.
                                    InsertPosition(set, character, idx);
                                    character[idx++] = 56;
                                    dataLength++;
                                    idx++;
                                }
                            }

                            else
                            {
                                // Shift A.
                                InsertPosition(set, character, idx);
                                character[idx++] = 59;
                                dataLength++;
                            }
                            break;

                        case 2:
                            if (set[idx + 1] == 2)
                            {
                                // Latch B.
                                InsertPosition(set, character, idx);
                                character[idx++] = 63;
                                currentSet = 2;
                                dataLength++;
                            }

                            else
                            {
                                // Shift B.
                                InsertPosition(set, character, idx);
                                character[idx++] = 59;
                                dataLength++;
                            }
                            break;

                        case 3:
                            // Shift C.
                            InsertPosition(set, character, idx);
                            character[idx++] = 60;
                            dataLength++;
                            break;

                        case 4:
                            // Shift D.
                            InsertPosition(set, character, idx);
                            character[idx++] = 61;
                            dataLength++;
                            break;

                        case 5:
                            // Shift E.
                            InsertPosition(set, character, idx);
                            character[idx++] = 62;
                            dataLength++;
                            break;

                        case 6:
                            // Number compressed, do nothing.
                            break;
                    }
                }

                idx++;
            } while (idx < 144);

            // Number compression.
            idx = 0;
            do
            {
                if (set[idx] == 6)
                {
                    string compressData = string.Empty;
                    for (int j = 0; j < 9; j++)
                        compressData += (char)character[idx + j];

                    int value = int.Parse(compressData, CultureInfo.CurrentCulture);

                    character[idx] = 31;    // NS.
                    character[idx + 1] = (value & 0x3f000000) >> 24;
                    character[idx + 2] = (value & 0xfc0000) >> 18;
                    character[idx + 3] = (value & 0x3f000) >> 12;
                    character[idx + 4] = (value & 0xfc0) >> 6;
                    character[idx + 5] = (value & 0x3f);

                    idx += 6;
                    for (int j = idx; j < 140; j++)
                    {
                        set[j] = set[j + 3];
                        character[j] = character[j + 3];
                    }

                    dataLength -= 3;
                }

                else
                    idx++;

            } while (idx < 144);

            // Insert ECI at the beginning of message if needed.
            // Encode ECI assignment numbers according to table 3.
            if (eci != 3)
            {
                InsertPosition(set, character, 0);
                character[0] = 27; // ECI
                if (eci <= 31)
                {
                    InsertPosition(set, character, 1);
                    character[1] = eci;
                    dataLength += 2;
                }

                if ((eci >= 32) && (eci <= 1023))
                {
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    character[1] = 0x20 + ((eci >> 6) & 0x0F);
                    character[2] = eci & 0x3F;
                    dataLength += 3;
                }

                if ((eci >= 1024) && (eci <= 32767))
                {
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    character[1] = 0x30 + ((eci >> 12) & 0x03);
                    character[2] = (eci >> 6) & 0x3F;
                    character[3] = eci & 0x3F;
                    dataLength += 4;
                }

                if (eci >= 32768)
                {
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    InsertPosition(set, character, 1);
                    character[1] = 0x38 + ((eci >> 18) & 0x02);
                    character[2] = (eci >> 12) & 0x3F;
                    character[3] = (eci >> 6) & 0x3F;
                    character[4] = eci & 0x3F;
                    dataLength += 5;
                }
            }

            switch (mode)
            {
                case MaxicodeMode.Mode2:
                case MaxicodeMode.Mode3:
                    if (dataLength > 84)
                        return false;

                    // Secondary only.
                    for (int i = 0; i < 84; i++)
                        maxiCodewords[i + 20] = character[i];

                    break;

                case MaxicodeMode.Mode4:
                case MaxicodeMode.Mode6:
                    if (dataLength > 93)
                        return false;

                    // Primary.
                    for (int i = 0; i < 9; i++)
                        maxiCodewords[i + 1] = character[i];

                    // Secondary.
                    for (int i = 0; i < 84; i++)
                        maxiCodewords[i + 20] = character[i + 9];

                    break;

                case MaxicodeMode.Mode5:
                    if (dataLength > 77)
                        return false;

                    // Primary.
                    for (int i = 0; i < 9; i++)
                        maxiCodewords[i + 1] = character[i];

                    // Secondary.
                    for (int i = 0; i < 68; i++)
                        maxiCodewords[i + 20] = character[i + 9];

                    break;
            }

            return true;
        }

        private static void InsertPosition(int[] set, int[] character, int position)
        {
            // Moves everything up so that a shift, latch or eci can be inserted.
            for (int i = 143; i > position; i--)
            {
                set[i] = set[i - 1];
                character[i] = character[i - 1];
            }
        }

        private static void PrimaryDataCheck(int[] maxiCodewords)
        {
            // Handles error correction of primary barcodeMessage.
            int dataLength = 10;
            int eccLength = 10;
            byte[] data = new byte[dataLength];
            byte[] result = new byte[eccLength];


            for (int j = 0; j < dataLength; j++)
                data[j] = (byte)maxiCodewords[j];

            ReedSolomon.RSInitialise(0x43, eccLength, 1);
            ReedSolomon.RSEncode(dataLength, data, result);

            for (int j = 0; j < eccLength; j++)
                maxiCodewords[dataLength + j] = result[eccLength - 1 - j];
        }

        private static void PrimaryMode2(char[] postcode, int countryCode, int serviceClass, int[] maxiCodewords)
        {
            int postcodeLength = postcode.Length;
            int postcodeValue = int.Parse(new string(postcode), CultureInfo.CurrentCulture);

            maxiCodewords[0] = ((postcodeValue & 0x03) << 4) | 2;
            maxiCodewords[1] = ((postcodeValue & 0xfc) >> 2);
            maxiCodewords[2] = ((postcodeValue & 0x3f00) >> 8);
            maxiCodewords[3] = ((postcodeValue & 0xfc000) >> 14);
            maxiCodewords[4] = ((postcodeValue & 0x3f00000) >> 20);
            maxiCodewords[5] = ((postcodeValue & 0x3c000000) >> 26) | ((postcodeLength & 0x3) << 4);
            maxiCodewords[6] = ((postcodeLength & 0x3c) >> 2) | ((countryCode & 0x3) << 4);
            maxiCodewords[7] = (countryCode & 0xfc) >> 2;
            maxiCodewords[8] = ((countryCode & 0x300) >> 8) | ((serviceClass & 0xf) << 2);
            maxiCodewords[9] = ((serviceClass & 0x3f0) >> 4);
        }

        private static void PrimaryMode3(char[] postcode, int countryCode, int serviceClass, int[] maxiCodewords)
        {
            // Format structured primary for Mode 3.
            for (int i = 0; i < postcode.Length; i++)
            {
                // (Capital) letters shifted to Code Set A values.
                if ((postcode[i] >= 'A') && (postcode[i] <= 'Z'))
                    postcode[i] -= (char)64;

                // Not a valid postcode character.
                if (((postcode[i] == 27) || (postcode[i] == 31)) || ((postcode[i] == 33) || (postcode[i] >= 59)))
                    postcode[i] = ' ';

                // Input characters lower than 27 (NUL - SUB) in postcode are
                // interpreted as capital letters in Code Set A (e.g. LF becomes 'J').
            }

            maxiCodewords[0] = ((postcode[5] & 0x03) << 4) | 3;
            maxiCodewords[1] = ((postcode[4] & 0x03) << 4) | ((postcode[5] & 0x3c) >> 2);
            maxiCodewords[2] = ((postcode[3] & 0x03) << 4) | ((postcode[4] & 0x3c) >> 2);
            maxiCodewords[3] = ((postcode[2] & 0x03) << 4) | ((postcode[3] & 0x3c) >> 2);
            maxiCodewords[4] = ((postcode[1] & 0x03) << 4) | ((postcode[2] & 0x3c) >> 2);
            maxiCodewords[5] = ((postcode[0] & 0x03) << 4) | ((postcode[1] & 0x3c) >> 2);
            maxiCodewords[6] = ((postcode[0] & 0x3c) >> 2) | ((countryCode & 0x3) << 4);
            maxiCodewords[7] = (countryCode & 0xfc) >> 2;
            maxiCodewords[8] = ((countryCode & 0x300) >> 8) | ((serviceClass & 0xf) << 2);
            maxiCodewords[9] = ((serviceClass & 0x3f0) >> 4);
        }

        private static void SecondaryDataCheckEven(int eccLength, int[] maxiCodewords)
        {
            // Handles error correction of even characters in secondary.
            int dataLength = 68;
            if (eccLength == 20)
                dataLength = 84;

            byte[] dataCodewords = new byte[dataLength];
            byte[] eccCodewords = new byte[eccLength];

            for (int j = 0; j < dataLength + 1; j++)
            {
                if ((j & 1) == 0) // Even
                    dataCodewords[j / 2] = (byte)maxiCodewords[j + 20];
            }

            ReedSolomon.RSInitialise(0x43, eccLength, 1);
            ReedSolomon.RSEncode(dataLength / 2, dataCodewords, eccCodewords);

            for (int j = 0; j < eccLength; j++)
                maxiCodewords[dataLength + (2 * j) + 20] = eccCodewords[eccLength - 1 - j];
        }

        private static void SecondaryDataCheckOdd(int eccLength, int[] maxiCodewords)
        {
            // Handles error correction of odd characters in secondary.
            int dataLength = 68;
            if (eccLength == 20)
                dataLength = 84;

            byte[] dataCodewords = new byte[dataLength];
            byte[] eccCodewords = new byte[eccLength];

            for (int j = 0; j < dataLength; j++)
            {
                if ((j & 1) != 0)  // Odd
                    dataCodewords[(j - 1) / 2] = (byte)maxiCodewords[j + 20];
            }

            ReedSolomon.RSInitialise(0x43, eccLength, 1);
            ReedSolomon.RSEncode(dataLength / 2, dataCodewords, eccCodewords);

            for (int j = 0; j < eccLength; j++)
                maxiCodewords[dataLength + (2 * j) + 1 + 20] = eccCodewords[eccLength - 1 - j];
        }

        private static void SetModule(int row, int column, int[,] symbolGrid)
        {
            symbolGrid[row, column / 7] |= 1 << (column % 7);
        }

        private static byte GetModule(int row, int column, int[,] symbolGrid)
        {
            return (byte)((symbolGrid[row, column / 7] >> (column % 7)) & 1);
        }

        // Iterates the array and test for numeric only characters.
        private static bool IsNumeric(char[] data, int position, int count)
        {
            if (position + count > data.Length)
                throw new ArgumentOutOfRangeException("position");

            for (int i = 0; i < count; i++)
            {
                if(!Char.IsDigit(data[i + position]))
                    return false;
            }

            return true;
        }
    }
}