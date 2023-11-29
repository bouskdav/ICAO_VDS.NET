using System;
using System.IO;
using System.Numerics;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities.Encoders;

namespace ICAO.VDSnet.Seals
{
    public class VdsSignature
    {
        private byte[] rawSignatureBytes;
        private byte[] signatureBytes = null;

        public VdsSignature(byte[] rawBytes)
        {
            rawSignatureBytes = rawBytes;
            ParseSignature(rawBytes);
        }

        public byte[] GetSignatureBytes()
        {
            return signatureBytes;
        }

        public byte[] GetRawSignatureBytes()
        {
            return rawSignatureBytes;
        }

        private void ParseSignature(byte[] rsBytes)
        {
            byte[] r = new byte[rsBytes.Length / 2];
            byte[] s = new byte[rsBytes.Length / 2];

            Buffer.BlockCopy(rsBytes, 0, r, 0, r.Length);
            Buffer.BlockCopy(rsBytes, r.Length, s, 0, s.Length);

            var rBigInt = new Org.BouncyCastle.Math.BigInteger(1, r);
            var sBigInt = new Org.BouncyCastle.Math.BigInteger(1, s);

            var v = new DerSequence(
                new DerInteger(rBigInt),
                new DerInteger(sBigInt)
            );

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var derOutputStream = new DerOutputStream(memoryStream);
                    derOutputStream.WriteObject(v);
                    signatureBytes = memoryStream.ToArray();
                    Console.WriteLine($"Signature sequence bytes: 0x{Hex.ToHexString(signatureBytes)}");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't parse r and s to signatureBytes.");
            }
        }
    }
}
