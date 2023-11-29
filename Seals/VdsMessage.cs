using System.Collections.Generic;

namespace ICAO.VDSnet.Seals
{
    public class VdsMessage
    {
        private byte[] rawBytes;
        private List<DocumentFeatureDto> documentFeatures = new List<DocumentFeatureDto>(5);

        public byte[] RawBytes
        {
            get { return rawBytes; }
        }

        public void AddDocumentFeature(DocumentFeatureDto docFeature)
        {
            documentFeatures.Add(docFeature);
        }

        public List<DocumentFeatureDto> GetDocumentFeatures()
        {
            return documentFeatures;
        }

        public void SetRawDataBytes(byte[] rawBytes)
        {
            this.rawBytes = rawBytes;
        }
    }
}
