using System.Collections.Generic;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class AddressStickerPass : DigitalSeal
    {
        public AddressStickerPass(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                        string postalCode = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.POSTAL_CODE] = postalCode;
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }
    }
}
