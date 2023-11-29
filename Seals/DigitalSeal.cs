using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Ocsp;
using ICAO.VDSnet;

namespace ICAO.VDSnet.Seals
{
    public abstract class DigitalSeal
    {
        private VdsType vdsType;
        private VdsHeader vdsHeader;
        private VdsMessage vdsMessage;
        private VdsSignature vdsSignature;
        private string rawString;
        protected Dictionary<Feature, object> featureMap = new Dictionary<Feature, object>();

        public DigitalSeal(VdsHeader vdsHeader, VdsMessage vdsMessage, VdsSignature vdsSignature)
        {
            this.vdsHeader = vdsHeader;
            this.vdsMessage = vdsMessage;
            this.vdsSignature = vdsSignature;
            vdsType = VdsTypeExtensions.ValueOf(vdsHeader.DocumentRef);
        }

        public static DigitalSeal GetInstance(string rawString)
        {
            DigitalSeal seal = DataParser.ParseVdsSeal(rawString);
            seal.rawString = rawString;
            return seal;
        }

        public VdsType GetVdsType()
        {
            return vdsType;
        }

        public List<DocumentFeatureDto> GetDocumentFeatures()
        {
            return vdsMessage.GetDocumentFeatures();
        }

        public Dictionary<Feature, object> GetFeatureMap()
        {
            return new Dictionary<Feature, object>(featureMap);
        }

        public string GetIssuingCountry()
        {
            return vdsHeader.IssuingCountry;
        }

        public string GetSignerCertRef()
        {
            int certRefInteger = int.Parse(vdsHeader.CertificateReference, System.Globalization.NumberStyles.HexNumber);
            return $"{vdsHeader.SignerIdentifier}{certRefInteger:X}".ToUpper();
        }

        public string GetSignerIdentifier()
        {
            return vdsHeader.SignerIdentifier;
        }

        public string GetCertificateReference()
        {
            return vdsHeader.CertificateReference;
        }

        public DateTime GetIssuingDate()
        {
            return vdsHeader.IssuingDate;
        }

        public DateTime GetSigDate()
        {
            return vdsHeader.SigDate;
        }

        public byte GetDocFeatureRef()
        {
            return vdsHeader.DocFeatureRef;
        }

        public byte GetDocTypeCat()
        {
            return vdsHeader.DocTypeCat;
        }

        public byte[] GetHeaderAndMessageBytes()
        {
            return vdsHeader.RawBytes.Concat(vdsMessage.RawBytes).ToArray();
        }

        public byte[] GetSignatureBytes()
        {
            return vdsSignature.GetSignatureBytes();
        }

        public string GetRawString()
        {
            return rawString;
        }

        public object GetFeature(Feature feature)
        {
            try
            {
                return featureMap[feature];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public T GetFeature<T>(Feature feature)
        {
            //try
            //{
            //    return featureMap[feature];
            //}
            //catch (Exception)
            //{
            //    return null;
            //}

            if (feature is T)
            {
                return (T)featureMap[feature];
            }
            try
            {
                return (T)Convert.ChangeType(featureMap[feature], typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }
    }
}
