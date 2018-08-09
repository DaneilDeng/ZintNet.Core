/* SymbolDictionary.cs - Symbol Names and ID's for ZintNet */

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

using System.Collections.Generic;

namespace ZintNet
{
    internal static class SymbolDictionary
    {
        private struct Descriptor
        {
            Symbology id;
            string group;

            public Descriptor(Symbology id, string group)
            {
                this.id = id;
                this.group = group;
            }
        }

        static IDictionary<string, Symbology> symbolDictionary = new Dictionary<string, Symbology>()
        {
            {"Code One", Symbology.CodeOne},
            {"Code 39 (ISO 16388)", Symbology.Code39},
            {"Code 39 Extended", Symbology.Code39Extended},
            {"LOGMARS", Symbology.LOGMARS},
            {"Code 32 (Italian Pharmacode)", Symbology.Code32},
            {"Pharmazentral Nummer (PZN)", Symbology.PharmaZentralNummer},
            {"Pharmacode", Symbology.Pharmacode},
            {"Pharmacode 2-Track", Symbology.Pharmacode2Track},
            {"Code 93", Symbology.Code93},
            {"Channel Code", Symbology.ChannelCode},
            {"Telepen", Symbology.Telepen},
            {"Telepen Numeric", Symbology.TelepenNumeric},
            {"Code 128 (ISO 15417)", Symbology.Code128},
            {"EAN-14", Symbology.EAN14},
            {"SSCC 18", Symbology.SSCC18},
            {"Code 2of5 Standard", Symbology.Standard2of5},
            {"Code 2of5 Interleaved", Symbology.Interleaved2of5},
            {"Code 2of5 Matrix", Symbology.Matrix2of5},
            {"Code 2of5 IATA", Symbology.IATA2of5},
            {"Code 2of5 Data Logic", Symbology.DataLogic2of5},
            {"ITF-14", Symbology.ITF14},
            {"Deutsche Post Identcode", Symbology.DeutschePostIdentCode},
            {"Deutshe Post Leitcode", Symbology.DeutshePostLeitCode},
            {"Codabar", Symbology.Codabar},
            {"MSI Plessey", Symbology.MSIPlessey},
            {"UK Plessey", Symbology.UKPlessey},
            {"Code 11", Symbology.Code11},
            {"ISBN", Symbology.ISBN},
            {"EAN-13", Symbology.EAN13},
            {"EAN-8", Symbology.EAN8},
            {"UPC-A", Symbology.UPCA},
            {"UPC-E", Symbology.UPCE},
            {"Databar Omnidirectional", Symbology.DatabarOmni},
            {"Databar Omnidirectional Stacked", Symbology.DatabarOmniStacked},
            {"Databar Stacked", Symbology.DatabarStacked},
            {"Databar Truncated", Symbology.DatabarTruncated},
            {"Databar Limited", Symbology.DatabarLimited},
            {"Databar Expanded", Symbology.DatabarExpanded},
            {"Databar Expanded Stacked", Symbology.DatabarExpandedStacked},
            {"Data Matrix (ISO 16022)", Symbology.DataMatrix},
            {"QR Code (ISO 18004)", Symbology.QRCode},
            {"Micro QR Code", Symbology.MicroQRCode},
            {"UPN QR Code", Symbology.UPNQR},
            {"Aztec Code (ISO 24778)", Symbology.Aztec},
            {"Aztec Runes", Symbology.AztecRunes},
            {"Maxicode(ISO 16023)", Symbology.MaxiCode},
            {"PDF 417 (ISO 15438)", Symbology.PDF417},
            {"PDF 417 Truncated", Symbology.PDF417Truncated},
            {"Micro PDF 417 (ISO 24728)", Symbology.MicroPDF417},
            {"Australia Post Standard Customer", Symbology.AusPostStandard},
            {"Australia Post Reply Paid", Symbology.AusPostReplyPaid},
            {"Australia Post Redirect", Symbology.AusPostRedirect},
            {"Australia Post Routing", Symbology.AusPostRouting},
            {"USPS Intelligent Mail", Symbology.USPS},
            {"PostNet Code", Symbology.PostNet},
            {"Planet Code", Symbology.Planet},
            {"Korean Postal", Symbology.KoreaPost},
            {"Facing Indentifcation Mark (FIM)", Symbology.FIM},
            {"Royal Mail 4 State Barcode", Symbology.RoyalMail},
            {"Dutch Post (KIX)", Symbology.KixCode},
            {"DAFT Code", Symbology.DaftCode},
            {"Flattermarken", Symbology.Flattermarken},
            {"Japanese Postal", Symbology.JapanPost},
            {"Codablock-F", Symbology.CodablockF},
            {"Code 16K", Symbology.Code16K},
            {"Dot Code", Symbology.DotCode},
            {"Grid Matrix", Symbology.GridMatrix},
            {"Code 49", Symbology.Code49},
            {"Han Xin Code", Symbology.HanXin},
            {"VIN Code", Symbology.VINCode},
            {"Royal Mail 4 State Mailmark", Symbology.RoyalMailMailmark}
        };

        public static Symbology GetSymbolId(string symbolName)
        {
            // Try to get the symbol id in the static dictionary.
            Symbology symbolId;
            if (symbolDictionary.TryGetValue(symbolName, out symbolId))
                return symbolId;

            else
                return Symbology.Invalid;
        }

        public static string[] GetSymbolNames()
        {
            string[] symbolNames = new string[symbolDictionary.Count];
            int index = 0;

            foreach (KeyValuePair<string, Symbology> entry in symbolDictionary)
            {
                if (entry.Key != "Invalid")
                    symbolNames[index] = entry.Key;

                index++;
            }

            return symbolNames;
        }

        public static string GetSymbolName(Symbology symbolId)
        {
            foreach (KeyValuePair<string, Symbology> entry in symbolDictionary)
            {
                if (entry.Value == symbolId)
                    return entry.Key;
            }

            return string.Empty;
        }


    }
}
