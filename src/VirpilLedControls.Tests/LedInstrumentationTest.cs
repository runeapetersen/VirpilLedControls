using System.Linq;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class LedInstrumentationTest
    {
        [Fact(Skip = "This test is for instrumentation purposes only and requires a Virpil device to be connected.")]
        public void CallHidDevice()
        {
            var hidDevice = HidLibrary.HidDevices.Enumerate(VirpilDevice.VendorId).FirstOrDefault(d =>
                d.ProductId == 0x4259  &&
                d.Capabilities.FeatureReportByteLength > 0);
            
            var packet = PacketHandling.CreatePacket(PacketHandling.BoardType.ResetToDefaultsReserved, 1, ColorIntensity.Off, ColorIntensity.Off, ColorIntensity.Sixty);
            TestContext.Current.TestOutputHelper.Write($"Packet: {string.Join(", ", packet.Select(b => b.ToString("X2")))}");
            hidDevice.WriteFeatureData(packet);
        }
    }
}