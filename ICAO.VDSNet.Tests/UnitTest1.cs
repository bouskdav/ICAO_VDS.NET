using ICAO.VDSnet;
using ICAO.VDSnet.Seals;
using System.Data;

namespace ICAO.VDSNet.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // vízum z webu 2
            byte[] vdsData = Convert.FromHexString("DC036D336D32C8A72CB19BABA6AC4D77FB060230DD7913515C926EC066D417B59E8C6ABC133C133C133C133C3FEF3A2938EE43F1593D1AE52DBB26751FE64B7C133C136B0306D79519A73374FF384D546B9D6490C414ED6042DD8F27831BD6DB9705A4D91AF15D563124749295EC649BCCDB54115683DF5D1FE21E608EB2791090DECCED7E6D");

            // complex implementation
            DigitalSeal digitalSeal = DataParser.ParseVdsSeal(vdsData);
            VdsType vdsType = digitalSeal.GetVdsType();

            string mrz = digitalSeal.GetFeature<string>(Feature.MRZ);
        }
    }
}