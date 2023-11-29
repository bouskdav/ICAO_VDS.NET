using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class AliensLaw : DigitalSeal
    {
        public AliensLaw(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                        byte[] faceImage = feature.GetValue();
                        featureMap[Feature.FACE_IMAGE] = faceImage;
                        break;
                    case 0x02:
                        string shortMraz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                        string mraz = string.Format("{0,-72}", shortMraz).Replace(' ', '<');
                        StringBuilder sb = new StringBuilder(mraz);
                        sb.Insert(36, '\n');
                        featureMap[Feature.MRZ] = sb.ToString();
                        break;
                    case 0x03:
                        string passportNumber = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.PASSPORT_NUMBER] = passportNumber;
                        break;
                    case 0x04:
                        string azr = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.AZR] = azr;
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }
    }
}
