using System.Collections.Generic;
using SPAD.neXt.Interfaces.HID;

public interface IHidDevices
{
    IEnumerable<IHidDevice> Enumerate(int vendorId);
}