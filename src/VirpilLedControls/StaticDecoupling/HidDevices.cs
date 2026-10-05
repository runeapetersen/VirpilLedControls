using System.Collections.Generic;
using SPAD.neXt.Interfaces.HID;

namespace VirpilLedControls.StaticDecoupling
{
    public class HidDevices : VirpilLedControls.Interfaces.IHidDevices
    {
        public IEnumerable<IHidDevice> Enumerate(int vendorId)
        {
            return HidLibrary.HidDevices.Enumerate(vendorId);
        }
    }
}