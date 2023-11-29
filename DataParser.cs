using ICAO.VDSnet.Extensions;
using ICAO.VDSnet.Seals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ICAO.VDSnet
{
    public static class DataParser
    {
        private const string C40CharTable = "    0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //                                  "0123456789012345678901234567890123456789";

        public static DigitalSeal ParseVdsSeal(string rawString)
        {
            byte[] rawBytes = DecodeBase256(rawString);
            Console.WriteLine($"rawString: {rawString}");
            return ParseVdsSeal(rawBytes);
        }

        public static DigitalSeal ParseVdsSeal(byte[] rawBytes)
        {
            ByteBuffer rawData = new ByteBuffer(rawBytes);
            Console.WriteLine($"rawData: {BitConverter.ToString(rawBytes)}");

            VdsHeader vdsHeader = DecodeHeader(rawData);
            VdsMessage vdsMessage = new VdsMessage();
            VdsSignature vdsSignature = null;

            int messageStartPosition = rawData.Position;
            int signatureStartPosition = 0;

            while (rawData.HasRemaining)
            {
                int tag = rawData.Get() & 0xFF;
                if (tag == 0xFF)
                {
                    signatureStartPosition = rawData.Position - 1;
                }

                int le = rawData.Get() & 0xFF;
                if (le == 0x81)
                {
                    le = rawData.Get() & 0xFF;
                }
                else if (le == 0x82)
                {
                    le = (rawData.Get() & 0xFF) * 0x100 + (rawData.Get() & 0xFF);
                }
                else if (le == 0x83)
                {
                    le = (rawData.Get() & 0xFF) * 0x1000 + (rawData.Get() & 0xFF) * 0x100 + (rawData.Get() & 0xFF);
                }
                else if (le > 0x7F)
                {
                    Console.WriteLine($"can't decode length: 0x{le:X2}");
                    throw new ArgumentException($"can't decode length: 0x{le:X2}");
                }

                byte[] val = GetFromByteBuffer(rawData, le);

                if (tag == 0xFF)
                {
                    vdsSignature = new VdsSignature(val);

                    vdsMessage.SetRawDataBytes(rawData.Array.Skip(messageStartPosition).Take(signatureStartPosition - messageStartPosition).ToArray());
                    //vdsMessage.SetRawDataBytes(val);

                    break;
                }

                vdsMessage.AddDocumentFeature(new DocumentFeatureDto((byte)(tag & 0xFF), le, val));
            }

            VdsType vdsType = (VdsType)vdsHeader.DocumentRef;
            switch (vdsType)
            {
                case VdsType.ARRIVAL_ATTESTATION:
                    return new ArrivalAttestation(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.SOCIAL_INSURANCE_CARD:
                    return new SocialInsuranceCard(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.ICAO_VISA:
                    return new IcaoVisa(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.RESIDENCE_PERMIT:
                    return new ResidencePermit(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.ICAO_EMERGENCY_TRAVEL_DOCUMENT:
                    return new IcaoEmergencyTravelDocument(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.SUPPLEMENTARY_SHEET:
                    return new SupplementarySheet(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.ADDRESS_STICKER_ID:
                    return new AddressStickerIdCard(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.ADDRESS_STICKER_PASSPORT:
                    return new AddressStickerPass(vdsHeader, vdsMessage, vdsSignature);
                case VdsType.ALIENS_LAW:
                    return new AliensLaw(vdsHeader, vdsMessage, vdsSignature);
                default:
                    Console.WriteLine($"unknown VDS type with reference: 0x{vdsHeader.DocumentRef:X2}");
                    return null;
            }
        }

        public static VdsHeader DecodeHeader(ByteBuffer rawdata)
        {
            int magicByte = rawdata.Get();
            if (magicByte != 0xDC)
            {
                Console.WriteLine($"Magic Constant mismatch: 0x{magicByte:X2} instead of 0xDC");
                throw new ArgumentException($"Magic Constant mismatch: 0x{magicByte:X2} instead of 0xDC");
            }

            VdsHeader vdsHeader = new VdsHeader();

            vdsHeader.RawVersion = rawdata.Get();

            if (!(vdsHeader.RawVersion == 0x02 || vdsHeader.RawVersion == 0x03))
            {
                Console.WriteLine($"Unsupported rawVersion: 0x{vdsHeader.RawVersion:X2}");
                throw new ArgumentException($"Unsupported rawVersion: 0x{vdsHeader.RawVersion:X2}");
            }

            vdsHeader.IssuingCountry = DecodeC40(GetFromByteBuffer(rawdata, 2));

            rawdata.Mark();

            string signerIdentifierAndCertRefLength = DecodeC40(GetFromByteBuffer(rawdata, 4));
            vdsHeader.SignerIdentifier = signerIdentifierAndCertRefLength.Substring(0, 4);

            if (vdsHeader.RawVersion == 0x03)
            {
                int certRefLength = int.Parse(signerIdentifierAndCertRefLength.Substring(4), NumberStyles.HexNumber);
                Console.WriteLine($"version 4: certRefLength: {certRefLength}");

                bool gaadHack = vdsHeader.SignerIdentifier.Equals("DEME") || vdsHeader.SignerIdentifier.Equals("DES1");
                if (gaadHack)
                {
                    Console.WriteLine("Maybe we found a German Arrival Attestation. GAAD Hack will be applied!");
                    certRefLength = 3;
                }

                int bytesToDecode = (certRefLength - 1) / 3 * 2 + 2;
                Console.WriteLine($"version 4: bytesToDecode: {bytesToDecode}");
                vdsHeader.CertificateReference = DecodeC40(GetFromByteBuffer(rawdata, bytesToDecode));
                if (gaadHack)
                {
                    vdsHeader.CertificateReference = signerIdentifierAndCertRefLength.Substring(4) + vdsHeader.CertificateReference;
                }
            }
            else
            {
                rawdata.Reset();
                string signerCertRef = DecodeC40(GetFromByteBuffer(rawdata, 6));
                vdsHeader.CertificateReference = signerCertRef.Substring(4);
            }

            vdsHeader.IssuingDate = DecodeDate(GetFromByteBuffer(rawdata, 3));
            vdsHeader.SigDate = DecodeDate(GetFromByteBuffer(rawdata, 3));
            vdsHeader.DocFeatureRef = rawdata.Get();
            vdsHeader.DocTypeCat = rawdata.Get();
            vdsHeader.RawBytes = rawdata.Array.Take(rawdata.Position).ToArray();

            Console.WriteLine($"VdsHeader: {vdsHeader}");
            return vdsHeader;
        }

        private static byte[] GetFromByteBuffer(ByteBuffer buffer, int size)
        {
            byte[] tmpByteArray = new byte[size];
            if (buffer.Position + size <= buffer.Capacity)
            {
                tmpByteArray = buffer.Get(size);
            }
            return tmpByteArray;
        }

        private static DateTime DecodeDate(byte[] bytes)
        {
            if (bytes.Length != 3)
            {
                throw new ArgumentException("expected three bytes for date decoding");
            }

            //long intval = (long)ToUnsignedInt(bytes[0]) * 256 * 256 + ToUnsignedInt(bytes[1]) * 256L + ToUnsignedInt(bytes[2]);
            //int day = (int)((intval % 1000000) / 10000);
            //int month = (int)(intval / 1000000);
            //int year = (int)(intval % 10000);

            //return new DateTime(year, month, day);

            return bytes.GetDate();
        }

        private static int ToUnsignedInt(byte value)
        {
            return (value & 0x7F) + (value < 0 ? 128 : 0);
        }

        public static string DecodeC40(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int idx = 0; idx < bytes.Length; idx++)
            {
                if (idx % 2 == 0)
                {
                    byte i1 = bytes[idx];
                    byte i2 = bytes[idx + 1];

                    //uint i1u = (uint)i1;
                    int i1u = i1;
                    //uint i2u = (uint)i2;
                    int i2u = i2;

                    if (i1 == 0xFE)
                    {
                        sb.Append((char)(i2 - 1));
                    }
                    else
                    {
                        int v16 = i1u * 256 + i2u;
                        var u1 = (v16 - 1) / 1600;
                        var u2 = (v16 - u1 * 1600 - 1) / 40;
                        var u3 = v16 - u1 * 1600 - u2 * 40 - 1;

                        if (u1 != 0)
                        {
                            sb.Append(C40CharTable[u1]);
                        }
                        if (u2 != 0)
                        {
                            sb.Append(C40CharTable[u2]);
                        }
                        if (u3 != 0)
                        {
                            sb.Append(C40CharTable[u3]);
                        }
                    }

                    //if (i1 == 0xFE)
                    //{
                    //    sb.Append((char)(i2 - 1));
                    //}
                    //else
                    //{
                    //    int v16 = (ToUnsignedInt(i1) << 8) + ToUnsignedInt(i2) - 1;
                    //    int temp = v16 / 1600;
                    //    int u1 = temp;
                    //    v16 -= temp * 1600;
                    //    temp = v16 / 40;
                    //    int u2 = temp;
                    //    int u3 = v16 - temp * 40;

                    //    if (u1 != 0)
                    //    {
                    //        sb.Append(ToChar(u1));
                    //    }
                    //    if (u2 != 0)
                    //    {
                    //        sb.Append(ToChar(u2));
                    //    }
                    //    if (u3 != 0)
                    //    {
                    //        sb.Append(ToChar(u3));
                    //    }
                    //}
                }
            }
            return sb.ToString();
        }

        private static char ToChar(int intValue)
        {
            if (intValue == 3)
            {
                return (char)32;
            }
            else if (intValue >= 4 && intValue <= 13)
            {
                return (char)(intValue + 44);
            }
            else if (intValue >= 14 && intValue <= 39)
            {
                return (char)(intValue + 51);
            }

            // if character is unknown return "?"
            return (char)63;
        }

        public static byte[] DecodeBase256(string s)
        {
            char[] ca = s.ToCharArray();
            byte[] ba = new byte[ca.Length];
            for (int i = 0; i < ba.Length; i++)
            {
                ba[i] = (byte)ca[i];
            }
            return ba;
        }
    }

    public class ByteBuffer
    {
        private readonly byte[] array;
        private int position;
        private int markedPosition = 0;

        public ByteBuffer(byte[] array)
        {
            this.array = array ?? throw new ArgumentNullException(nameof(array));
            position = 0;
        }

        public byte[] Array => array;

        public int Position
        {
            get => position;
            set
            {
                if (value < 0 || value > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                position = value;
            }
        }

        public int Capacity => array.Length;

        public int Remaining => Capacity - Position;

        public bool HasRemaining => position < array.Length;

        public byte Get()
        {
            if (!HasRemaining)
            {
                throw new IndexOutOfRangeException("Buffer underflow");
            }
            return array[position++];
        }

        public void Mark()
        {
            markedPosition = Position;
        }

        public void Reset()
        {
            Position = markedPosition;
        }

        public byte[] Get(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (position + length > array.Length)
            {
                throw new IndexOutOfRangeException("Buffer underflow");
            }

            byte[] result = array.Skip(position).Take(length).ToArray();
            position += length;
            return result;
        }
    }
}
