using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class IcaoEmergencyTravelDocument : DigitalSeal
    {
        public IcaoEmergencyTravelDocument(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
            : base(vdsHeader, vdsMessage, vdsSignature)
        {
            ParseDocumentFeatures(vdsMessage.GetDocumentFeatures());
        }

        private void ParseDocumentFeatures(List<DocumentFeatureDto> features)
        {
            foreach (DocumentFeatureDto feature in features)
            {
                if (feature.GetTag() == 0x02)
                {
                    string mrz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                    StringBuilder sb = new StringBuilder(mrz);
                    sb.Insert(36, '\n');
                    featureMap[Feature.MRZ] = sb.ToString();
                }
                else
                {
                    Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                }
            }
        }
    }
}
