using System.Collections.Generic;
using SPAD.neXt.Interfaces.HID;

namespace VirpilLedControls.Interfaces
{
    public interface IHidDevices
    {
        IEnumerable<IHidDevice> Enumerate(int vendorId);
    }
}