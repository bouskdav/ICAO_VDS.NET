using ICAO.VDSnet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICAO.VDSnet.Extensions
{
    public static class VDSExtensions
    {
        public static string GetString(this Dictionary<byte, byte[]> dictionary, byte key)
        {
            if (dictionary.TryGetValue(key, out byte[] value))
            {
                return Encoding.UTF8.GetString(value);
            }
            else
            {
                throw new ArgumentException($"Missing tag {key}");
            }
        }

        public static DateTime GetTimestampFromUInt32(this byte[] byteArray)
        {
            if (byteArray.Length != 4)
            {
                throw new ArgumentException("Insufficient length for UInt32");
            }

            uint secondsSince1970 = (uint)(
                byteArray[0] << 24 |
                byteArray[1] << 16 |
                byteArray[2] << 8 |
                byteArray[3]
            );

            return DateTimeOffset.FromUnixTimeSeconds(secondsSince1970).DateTime;
        }

        public static DateTime GetDate(this byte[] byteArray)
        {
            if (byteArray.Length != 3)
            {
                throw new ArgumentException("Insufficient length for UInt24");
            }

            int value = DecodeUInt24BigEndian(byteArray[0], byteArray[1], byteArray[2]);
            return GetDateFromUInt24(value);
        }

        public static DateTime GetDate(this ByteBuffer byteBuffer)
        {
            if (byteBuffer.Remaining < 3)
            {
                throw new ArgumentException("Insufficient length for UInt24");
            }

            int value = DecodeUInt24BigEndian(byteBuffer.Get(), byteBuffer.Get(), byteBuffer.Get());
            return GetDateFromUInt24(value);
        }

        private static int DecodeUInt24BigEndian(byte high, byte middle, byte low)
        {
            return high << 16 | middle << 8 | low;
        }

        private static DateTime GetDateFromUInt24(int value)
        {
            string dateString = value.ToString("D8");
            if (DateTime.TryParseExact(dateString, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                throw new Exception();
                //return null;
            }
        }

        public static int GetUInt16LittleEndian(this byte[] byteArray)
        {
            if (byteArray.Length != 2)
            {
                throw new ArgumentException("Insufficient length for UInt16");
            }

            return byteArray[1] << 8 | byteArray[0];
        }

        public static int GetUInt24LittleEndian(this byte[] byteArray)
        {
            if (byteArray.Length != 3)
            {
                throw new ArgumentException("Insufficient length for UInt24");
            }

            return byteArray[2] << 16 | byteArray[1] << 8 | byteArray[0];
        }

        public static int GetUInt32LittleEndian(this byte[] byteArray)
        {
            if (byteArray.Length != 4)
            {
                throw new ArgumentException("Insufficient length for UInt32");
            }

            return byteArray[3] << 24 | byteArray[2] << 16 | byteArray[1] << 8 | byteArray[0];
        }
    }
}
