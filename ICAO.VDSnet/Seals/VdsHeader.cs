using System;

namespace ICAO.VDSnet.Seals
{
    public class VdsHeader
    {
        public byte[] RawBytes { get; set; }

        public string IssuingCountry { get; set; }
        public string SignerIdentifier { get; set; }
        public string CertificateReference { get; set; }
        public DateTime IssuingDate { get; set; }
        public DateTime SigDate { get; set; }

        public byte DocFeatureRef { get; set; }
        public byte DocTypeCat { get; set; }

        public byte RawVersion { get; set; }

        public int DocumentRef => ((DocFeatureRef & 0xFF) << 8) + (DocTypeCat & 0xFF);

        public override string ToString()
        {
            return $"rawVersion: {RawVersion & 0xff}\nissuingCountry: {IssuingCountry}\nsignerIdentifier: {SignerIdentifier}" +
                   $"\ncertificateReference: {CertificateReference}\nissuingDate: {IssuingDate}\nsigDate: {SigDate}" +
                   $"\ndocFeatureRef: {DocFeatureRef:X2}, docTypeCat: {DocTypeCat:X2}";
        }
    }
}
