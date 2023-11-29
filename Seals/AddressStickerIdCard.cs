using System;
using System.Collections.Generic;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class AddressStickerIdCard : DigitalSeal
    {
        public AddressStickerIdCard(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
            : base(vdsHeader, vdsMessage, vdsSignature)
        {
            ParseDocumentFeatures(vdsMessage.GetDocumentFeatures());
        }

        private void ParseDocumentFeatures(List<DocumentFeatureDto> features)
        {
            foreach (DocumentFeatureDto feature in features)
            {
                switch (feature.GetTag())
                {
                    case 0x01:
                        string docNr = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.DOCUMENT_NUMBER] = docNr;
                        break;
                    case 0x02:
                        string ags = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.AGS] = ags;
                        break;
                    case 0x03:
                        string rawAddress = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.RAW_ADDRESS] = rawAddress;
                        ParseAddress(rawAddress);
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }

        private void ParseAddress(string rawAddress)
        {
            string postalCode = rawAddress.Substring(0, 5);
            string street = System.Text.RegularExpressions.Regex.Replace(rawAddress.Substring(5), @"(\d+\w+)(?!.*\d)", "");
            string streetNr = System.Text.RegularExpressions.Regex.Replace(rawAddress.Substring(5), street, "");

            featureMap[Feature.POSTAL_CODE] = postalCode;
            featureMap[Feature.STREET] = street;
            featureMap[Feature.STREET_NR] = streetNr;

            Console.WriteLine($"parsed address: {postalCode}:{street}:{streetNr}");
        }
    }
}
