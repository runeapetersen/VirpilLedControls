using SPAD.neXt.Interfaces;

namespace VirpilLedControls.Interfaces
{
    public interface ILockFactory
    {
        ISmartLock CreateLock(string name);
    }
}