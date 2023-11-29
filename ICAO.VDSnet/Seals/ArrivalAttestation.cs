using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class ArrivalAttestation : DigitalSeal
    {
        public ArrivalAttestation(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                    case 0x02:
                        string mrz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                        StringBuilder sb = new StringBuilder(mrz);
                        sb.Insert(36, '\n');
                        featureMap[Feature.MRZ] = sb.ToString();
                        break;
                    case 0x03:
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
