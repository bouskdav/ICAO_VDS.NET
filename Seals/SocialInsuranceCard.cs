using System;
using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class SocialInsuranceCard : DigitalSeal
    {
        public SocialInsuranceCard(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                        string socialInsuranceNumber = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.SOCIAL_INSURANCE_NUMBER] = socialInsuranceNumber;
                        break;
                    case 0x02:
                        string surName = Encoding.UTF8.GetString(feature.GetValue());
                        featureMap[Feature.SURNAME] = surName;
                        break;
                    case 0x03:
                        string firstName = Encoding.UTF8.GetString(feature.GetValue());
                        featureMap[Feature.FIRST_NAME] = firstName;
                        break;
                    case 0x04:
                        string birthName = Encoding.UTF8.GetString(feature.GetValue());
                        featureMap[Feature.BIRTH_NAME] = birthName;
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }
    }
}
