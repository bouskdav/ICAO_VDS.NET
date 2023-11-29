using System;

namespace ICAO.VDSnet.Seals
{
    public class DocumentFeatureDto
    {
        private readonly byte tag;
        private readonly int length;
        private readonly byte[] value;

        public DocumentFeatureDto(byte tag, int len, byte[] val)
        {
            this.tag = tag;
            length = len;
            value = (byte[])val.Clone();
        }

        public byte GetTag()
        {
            return tag;
        }

        public byte[] GetValue()
        {
            return value;
        }

        public override string ToString()
        {
            return " T: " + $"{tag:X2}" + " L: " + $"{length:X2}" + " V: " + BitConverter.ToString(value).Replace("-", "");
        }
    }
}
