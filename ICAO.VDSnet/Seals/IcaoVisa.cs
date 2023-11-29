using System;
using System.Collections.Generic;
using System.Text;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public class IcaoVisa : DigitalSeal
    {
        public IcaoVisa(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
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
                        // MRZ chars per line: 44
                        string shortMrz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                        // fill mrz to the full length of 88 characters because ICAO cuts last 16 characters
                        string mrz = string.Format("{0,-88}", shortMrz).Replace(' ', '<');
                        StringBuilder sb = new StringBuilder(mrz);
                        sb.Insert(44, '\n');
                        featureMap[Feature.MRZ] = sb.ToString();
                        break;
                    case 0x02:
                        // MRZ chars per line: 36
                        shortMrz = DataParser.DecodeC40(feature.GetValue()).Replace(' ', '<');
                        // fill mrz to the full length of 72 characters because ICAO cuts last 8 characters
                        mrz = string.Format("{0,-72}", shortMrz).Replace(' ', '<');
                        sb = new StringBuilder(mrz);
                        sb.Insert(36, '\n');
                        featureMap[Feature.MRZ] = sb.ToString();
                        break;
                    case 0x03:
                        int numberOfEntries = feature.GetValue()[0] & 0xff;
                        featureMap[Feature.NUMBER_OF_ENTRIES] = numberOfEntries;
                        break;
                    case 0x04:
                        DecodeDuration(feature.GetValue());
                        break;
                    case 0x05:
                        string passportNumber = DataParser.DecodeC40(feature.GetValue());
                        featureMap[Feature.PASSPORT_NUMBER] = passportNumber;
                        break;
                    case 0x06:
                        byte[] visaType = feature.GetValue();
                        featureMap[Feature.VISA_TYPE] = visaType;
                        break;
                    case 0x07:
                        byte[] additionalFeatures = feature.GetValue();
                        featureMap[Feature.ADDITIONAL_FEATURES] = additionalFeatures;
                        break;
                    default:
                        Console.WriteLine("found unknown tag: 0x" + string.Format("{0:X2}", feature.GetTag()));
                        break;
                }
            }
        }

        private void DecodeDuration(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                throw new ArgumentException("expected three bytes for date decoding");
            }

            int durationOfStayDays = bytes[0] & 0xff;
            int durationOfStayMonths = bytes[1] & 0xff;
            int durationOfStayYears = bytes[2] & 0xff;

            featureMap[Feature.DURATION_OF_STAY_YEARS] = durationOfStayYears;
            featureMap[Feature.DURATION_OF_STAY_MONTHS] = durationOfStayMonths;
            featureMap[Feature.DURATION_OF_STAY_DAYS] = durationOfStayDays;
        }
    }
}
