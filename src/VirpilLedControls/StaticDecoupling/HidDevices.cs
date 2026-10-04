using System.Collections.Generic;
using SPAD.neXt.Interfaces.HID;

public class HidDevices : IHidDevices
{
    public IEnumerable<IHidDevice> Enumerate(int vendorId)
    {
        return HidLibrary.HidDevices.Enumerate(vendorId);
    }
}