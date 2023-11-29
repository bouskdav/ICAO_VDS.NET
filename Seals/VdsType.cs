using System;
using System.Collections.Generic;

namespace ICAO.VDSnet.Seals
{
    public enum VdsType
    {
        ARRIVAL_ATTESTATION = 0xfd02,
        ICAO_EMERGENCY_TRAVEL_DOCUMENT = 0x5e03,
        ICAO_VISA = 0x5d01,
        ADDRESS_STICKER_PASSPORT = 0xf80a,
        ADDRESS_STICKER_ID = 0xf908,
        RESIDENCE_PERMIT = 0xfb06,
        SOCIAL_INSURANCE_CARD = 0xfc04,
        SUPPLEMENTARY_SHEET = 0xfa06,
        ALIENS_LAW = 0x01fe
    }

    public static class VdsTypeExtensions
    {
        private static readonly Dictionary<int, VdsType> map = new Dictionary<int, VdsType>();

        static VdsTypeExtensions()
        {
            foreach (VdsType vdsType in Enum.GetValues(typeof(VdsType)))
            {
                map.Add((int)vdsType, vdsType);
            }
        }

        public static VdsType ValueOf(int vdsType)
        {
            return map.TryGetValue(vdsType, out var result) ? result : throw new ArgumentException($"Unknown VdsType value: {vdsType}");
        }
    }
}
