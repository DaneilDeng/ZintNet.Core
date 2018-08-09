/* BCEnums.cs - Enumerations for ZintNet */

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


namespace ZintNet
{
    /// <summary>
    /// Enumeration of supported Symbol ID's
    /// </summary>
	public enum Symbology : int
	{
        /// <summary>
        /// Code One 2D symbol.
        /// </summary>
        CodeOne = 0,
        /// <summary>
        /// Code 39 (ISO 16388)
        /// </summary>
		Code39,
        /// <summary>
        /// Code 39 extended ASCII.
        /// </summary>
		Code39Extended,
        /// <summary>
        /// Logistics Applications of Automated Marking and Reading Symbol.
        /// </summary>
        LOGMARS,
        /// <summary>
        /// Code 32 (Italian Pharmacode)
        /// </summary>
        Code32,
        /// <summary>
        /// Pharmazentralnummer (PZN - German Pharmaceutical Code)
        /// </summary>
        PharmaZentralNummer,
        /// <summary>
        /// Pharmaceutical Binary Code. 
        /// </summary>
        Pharmacode,
        /// <summary>
        /// Pharmaceutical Binary Code (2 Track)
        /// </summary>
        Pharmacode2Track,
        /// <summary>
        /// Code 93
        /// </summary>
		Code93,
        /// <summary>
        /// Channel Code.
        /// </summary>
        ChannelCode,
        /// <summary>
        /// Telepen Code.
        /// </summary>
        Telepen,
        /// <summary>
        /// Telepen Numeric Code.
        /// </summary>
        TelepenNumeric,
        /// <summary>
        /// Code 128/GS1-128 (ISO 15417)
        /// </summary>
		Code128,
        /// <summary>
        /// European Article Number (14)
        /// </summary>
        EAN14,
        /// <summary>
        /// Serial Shipping Container Code.
        /// </summary>
        SSCC18,
        /// <summary>
        /// Standard 2 of 5 Code.
        /// </summary>
		Standard2of5,
        /// <summary>
        /// Interleaved 2 of 5 Code.
        /// </summary>
		Interleaved2of5,
        /// <summary>
        /// Matrix 2 of 5 Code.
        /// </summary>
        Matrix2of5,
        /// <summary>
        /// IATA 2 of 5 Code.
        /// </summary>
        IATA2of5,
        /// <summary>
        /// Datalogic 2 of 5 Code.
        /// </summary>
        DataLogic2of5,
        /// <summary>
        /// ITF 14 (GS1 2 of 5 Code)
        /// </summary>
		ITF14,
        /// <summary>
        /// Deutsche Post Identcode (DHL)
        /// </summary>
        DeutschePostIdentCode,
        /// <summary>
        /// Deutsche Post Leitcode (DHL)
        /// </summary>
        DeutshePostLeitCode,
        /// <summary>
        /// Codabar Code.
        /// </summary>
		Codabar,
        /// <summary>
        /// MSI Plessey Code.
        /// </summary>
		MSIPlessey,
        /// <summary>
        /// UK Plessey Code.
        /// </summary>
        UKPlessey,
        /// <summary>
        /// Code 11.
        /// </summary>
		Code11,
        /// <summary>
        /// International Standard Book Number.
        /// </summary>
		ISBN,
        /// <summary>
        /// European Article Number (13)
        /// </summary>
		EAN13,
        /// <summary>
        /// European Article Number (8)
        /// </summary>
		EAN8,
        /// <summary>
        /// Universal Product Code (A)
        /// </summary>
		UPCA,
        /// <summary>
        /// Universal Product Code (E)
        /// </summary>
		UPCE,
        /// <summary>
        /// GS1 Databar Omnidirectional.
        /// </summary>
        DatabarOmni,
        /// <summary>
        /// GS1 Databar Omnidirectional Stacked.
        /// </summary>
        DatabarOmniStacked,
        /// <summary>
        /// GS1 Databar Stacked.
        /// </summary>
        DatabarStacked,
        /// <summary>
        /// GS1 Databar Omnidirectional Truncated.
        /// </summary>
        DatabarTruncated,
        /// <summary>
        /// GS1 Databar Limited.
        /// </summary>
        DatabarLimited,
        /// <summary>
        /// GS1 Databar Expanded.
        /// </summary>
        DatabarExpanded,
        /// <summary>
        /// GS1 Databar Expanded Stacked.
        /// </summary>
        DatabarExpandedStacked,
        /// <summary>
        /// Data Matrix (ISO 16022)
        /// </summary>
        DataMatrix,
        /// <summary>
        /// QR Code (ISO 18004)
        /// </summary>
        QRCode,
        /// <summary>
        /// Micro variation of QR Code.
        /// </summary>
        MicroQRCode,
        /// <summary>
        /// UPN variation of QR Code.
        /// </summary>
        UPNQR,
        /// <summary>
        /// Aztec (ISO 24778)
        /// </summary>
        Aztec,
        /// <summary>
        /// Aztec Runes.
        /// </summary>
        AztecRunes,
        /// <summary>
        /// Maxicode (ISO 16023)
        /// </summary>
        MaxiCode,
        /// <summary>
        /// PDF417 (ISO 15438)
        /// </summary>
        PDF417,
        /// <summary>
        /// PDF417 Truncated.
        /// </summary>
        PDF417Truncated,
        /// <summary>
        /// Micro PDF417 (ISO 24728)
        /// </summary>
        MicroPDF417,
        /// <summary>
        /// Australia Post Standard.
        /// </summary>
        AusPostStandard,
        /// <summary>
        /// Australia Post Reply Paid.
        /// </summary>
        AusPostReplyPaid,
        /// <summary>
        /// Australia Post Redirect.
        /// </summary>
        AusPostRedirect,
        /// <summary>
        /// Australia Post Routing.
        /// </summary>
        AusPostRouting,
        /// <summary>
        /// United States Postal Service Intelligent Mail.
        /// </summary>
        USPS,
        /// <summary>
        /// PostNET (Postal Numeric Encoding Technique)
        /// </summary>
        PostNet,
        /// <summary>
        /// Planet (Postal Alpha Numeric Encoding Technique)
        /// </summary>
        Planet,
        /// <summary>
        /// Korean Post.
        /// </summary>
        KoreaPost,
        /// <summary>
        /// Facing Identification Mark (FIM)
        /// </summary>
        FIM,
        /// <summary>
        /// UK Royal Mail 4 State Code.
        /// </summary>
        RoyalMail,
        /// <summary>
        /// KIX Dutch 4 State Code.
        /// </summary>
        KixCode,
        /// <summary>
        /// DAFT Code (Generic 4 State Code)
        /// </summary>
        DaftCode,
        /// <summary>
        /// Flattermarken (Markup Code)
        /// </summary>
        Flattermarken,
        /// <summary>
        /// Japanese Post.
        /// </summary>
        JapanPost,
        /// <summary>
        /// Codablock-F 2D symbol.
        /// </summary>
        CodablockF,
        /// <summary>
        /// Code 16K 2D symbol.
        /// </summary>
        Code16K,
        /// <summary>
        /// Dot Code 2D symbol.
        /// </summary>
        DotCode,
        /// <summary>
        /// Grid Matrix 2D symbol.
        /// </summary>
        GridMatrix,
        /// <summary>
        /// Code 49 2D symbol.
        /// </summary>
        Code49,
        /// <summary>
        /// Han Xin 2D symbol.
        /// </summary>
        HanXin,

        /// <summary>
        /// VIN code symbol.
        /// </summary>
        VINCode,

        /// <summary>
        /// Mailmark 4 state postal.
        /// </summary>
        RoyalMailMailmark,

        /// <summary>
        /// Not a valid Symbol ID.
        /// </summary>
        Invalid = -1
	}

    /// <summary>
    /// Enumeration of the barcodes text position.
    /// </summary>
	public enum TextPosition
	{
        /// <summary>
        /// Places the text under the barcode symbol.
        /// </summary>
		UnderBarcode = 0,
        /// <summary>
        /// Places the text above the barcode sysmbol.
        /// </summary>
		AboveBarcode
	}

    /// <summary>
    /// Enumeration of the barcodes text alignment.
    /// </summary>
	public enum TextAlignment
	{
        /// <summary>
        /// Displays the text centered within the symbol.
        /// </summary>
		Center = 0,

        /// <summary>
        /// Displays the text aligned to the left side of the symbol.
        /// </summary>
		Left,

        /// <summary>
        /// displays the text aligned to the right side of the symbol.
        /// </summary>
		Right,

        /// <summary>
        /// Expands the text across the full width of the symbol.
        /// </summary>
		Stretched
	}

    /// <summary>
    /// Enumeration of the ITF14 bearer style options.
    /// </summary>
    public enum ITF14BearerStyle
    {
        /// <summary>
        /// No bearer bars.
        /// </summary>
        None = 0,
        /// <summary>
        /// Horizontal bearer bars at the top and bottom of the symbol.
        /// </summary>
        Horizonal,
        /// <summary>
        /// Full rectangular bearer bars.
        /// </summary>
        Rectangle
    }

    /// <summary>
    /// Australian Post encoding modes.
    /// </summary>
    public enum AusPostEncoding
    {
        /// <summary>
        /// Numeric mode.
        /// </summary>
        Numeric = 0,
        /// <summary>
        /// Character mode.
        /// </summary>
        Character,
    }

    /// <summary>
    /// Enumeration of MSI code check digit types.
    /// </summary>
	public enum MSICheckDigitType
	{
        /// <summary>
        /// No check digit.
        /// </summary>
        None = 0,
        /// <summary>
        /// Modulus 10 check digit.
        /// </summary>
		Mod10,
        /// <summary>
        /// 2 Modulus 10 check digits.
        /// </summary>
        Mod10Mod10,
        /// <summary>
        /// Modulus 11 check digit.
        /// </summary>
		Mod11,
        /// <summary>
        /// Modulus 10 plus modulus 11 check digits.
        /// </summary>
        Mod11Mod10
	}

    /// <summary>
    /// Enumeration of Interleaved 2 of 5 check digit types.
    /// </summary>
	public enum I2of5CheckDigitType
	{
        /// <summary>
        /// Uniform Symbology Specification (USS) check digit.
        /// </summary>
		USS = 0,    // Default.
        /// <summary>
        /// Optical Product Code Council (OPCC) check digit.
        /// </summary>
		OPCC
		
	}

    /// <summary>
    /// Enumeration of the QR code error correction levels.
    /// </summary>
    public enum QRCodeErrorLevel
    {
        /// <summary>
        /// Low error level.
        /// </summary>
        Low = 0,
        /// <summary>
        /// Medium error level.
        /// </summary>
        Medium,
        /// <summary>
        /// Quartile level
        /// </summary>
        Quartile,
        /// <summary>
        /// High level.
        /// </summary>
        High
    }

    /// <summary>
    /// Enumeration of supported encoding modes.
    /// </summary>
    public enum EncodingMode
    {
        /// <summary>
        /// Standard encoding.
        /// </summary>
        Standard = 0,
        /// <summary>
        /// GS1 format.
        /// </summary>
        GS1,
        /// <summary>
        /// Health Industry Barcode format.
        /// </summary>
        HIBC
    }

    /// <summary>
    /// Enumeration of composite modes.
    /// </summary>
    public enum CompositeMode
    {
        /// <summary>
        /// Composite mode A.
        /// </summary>
        CCA = 0,
        /// <summary>
        /// Composite mode B.
        /// </summary>
        CCB,
        /// <summary>
        /// Composite mode C.
        /// </summary>
        CCC
    }

    /// <summary>
    /// Enumeration of Maxicode modes.
    /// </summary>
    public enum MaxicodeMode
    {
        /// <summary>
        /// Mode 2.
        /// </summary>
        Mode2 = 2,
        /// <summary>
        /// Mode 3.
        /// </summary>
        Mode3,
        /// <summary>
        /// Mode 4.
        /// </summary>
        Mode4,
        /// <summary>
        /// Mode 5.
        /// </summary>
        Mode5,
        /// <summary>
        /// Mode 6.
        /// </summary>
        Mode6
    }

    /// <summary>
    /// Enumeration of Datamatrix code symbol sizes.
    /// </summary>
    public enum DataMatrixSize : int
    {
        /// <summary>
        /// Automatic sizing.
        /// </summary>
        Automatic = 0,
        /// <summary>
        /// Datamatrix square 10x10.
        /// </summary>
        DM10X10,
        /// <summary>
        /// Datamatrix square 12x12.
        /// </summary>
        DM12X12,
        /// <summary>
        /// Datamatrix square 14 x 14.
        /// </summary>
        DM14X14,
        /// <summary>
        /// Datamatrix square 16 x 16.
        /// </summary>
        DM16X16,
        /// <summary>
        /// Datamatrix square 18 x 18.
        /// </summary>
        DM18X18,
        /// <summary>
        /// Datamatrix square 20 x 20.
        /// </summary>
        DM20X20,
        /// <summary>
        /// Datamatrix square 22 x 22.
        /// </summary>
        DM22X22,
        /// <summary>
        /// Datamatrix square 24 x 24.
        /// </summary>
        DM24X24,
        /// <summary>
        /// Datamatrix square 26 x 26.
        /// </summary>
        DM26X26,
        /// <summary>
        /// Datamatrix square 32 x 32.
        /// </summary>
        DM32X32,
        /// <summary>
        /// Datamatrix square 36 x 36.
        /// </summary>
        DM36X36,
        /// <summary>
        /// Datamatrix square 40 x 40.
        /// </summary>
        DM40X40,
        /// <summary>
        /// Datamatrix square 44 x 44.
        /// </summary>
        DM44X44,
        /// <summary>
        /// Datamatrix square 48 x 48.
        /// </summary>
        DM48X48,
        /// <summary>
        /// Datamatrix square 52 x 52.
        /// </summary>
        DM52X52,
        /// <summary>
        /// Datamatrix square 64 x 64.
        /// </summary>
        DM64X64,
        /// <summary>
        /// Datamatrix square 72 x 72.
        /// </summary>
        DM72X72,
        /// <summary>
        /// Datamatrix square 80 x 80.
        /// </summary>
        DM80X80,
        /// <summary>
        /// Datamatrix square 88 x 88.
        /// </summary>
        DM88X88,
        /// <summary>
        /// Datamatrix square 96 x 96.
        /// </summary>
        DM96X96,
        /// <summary>
        /// Datamatrix square 104 x 104.
        /// </summary>
        DM104X104,
        /// <summary>
        /// Datamatrix square 120 x 120.
        /// </summary>
        DM120X120,
        /// <summary>
        /// Datamatrix square 132 x 132.
        /// </summary>
        DM132X132,
        /// <summary>
        /// Datamatrix square 144 x144.
        /// </summary>
        DM144X144,
        /// <summary>
        /// Datamatrix rectangular 8 x 18.
        /// </summary>
        DM8X18,
        /// <summary>
        /// Datamatrix rectangular 8 x 32.
        /// </summary>
        DM8X32,
        /// <summary>
        /// Datamatrix rectangular 12 x 26.
        /// </summary>
        DM12X26,
        /// <summary>
        /// Datamatrix rectangular 12 x 36.
        /// </summary>
        DM12X36,
        /// <summary>
        /// Datamatrix rectangular 16 x 36.
        /// </summary>
        DM16X36,
        /// <summary>
        /// Datamatrix rectangular 16 x 48.
        /// </summary>
        DM16X48,
        /// <summary>
        /// Datamatrix rectangular extension 8 x 48;
        /// </summary>
        DM8X48,
        /// <summary>
        /// Datamatrix rectangular extension 8 x 64.
        /// </summary>
        DM8X64,
        /// <summary>
        /// Datamatrix rectangular extension 12 x 64.
        /// </summary>
        DM12X64,
        /// <summary>
        /// Datamatrix rectangular extension 16 x 64.
        /// </summary>
        DM16X64,
        // Not yet supported.
        // DM24X32,
        // DM24X36,
        /// <summary>
        /// Datamatrix rectangular extension 24 x 48.
        /// </summary>
        DM24X48,
        /// <summary>
        /// Datamatrix rectangular extension 24 x 64.
        /// </summary>
        DM24X64,
        // Not yet supported.
        // DM26X32,
        /// <summary>
        /// Datamatrix rectangular extension 26 x 40.
        /// </summary>
        DM26X40,
        /// <summary>
        /// Datamatrix rectangular extension 26 x 48.
        /// </summary>
        DM26X48,
        /// <summary>
        /// Datamatrix rectangular extension 26 x 64.
        /// </summary>
        DM26X64,
    }
}