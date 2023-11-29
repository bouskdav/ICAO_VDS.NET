using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class SupplementarySheet : DigitalSeal
    {
        public SupplementarySheet(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                    case 0x04:
                        string mrz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                        StringBuilder sb = new StringBuilder(mrz);
                        sb.Insert(36, '\n');
                        featureMap[Feature.MRZ] = sb.ToString();
                        break;
                    case 0x05:
                        string suppSheetNumber = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.SHEET_NUMBER] = suppSheetNumber;
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }
    }
}
