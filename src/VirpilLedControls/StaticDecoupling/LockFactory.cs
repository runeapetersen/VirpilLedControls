using SPAD.neXt.Interfaces;
using VirpilLedControls.Interfaces;

namespace VirpilLedControls.StaticDecoupling
{
    public class LockFactory : ILockFactory
    {
        public ISmartLock CreateLock(string name) => SpadSystem.ApplicationProxy.CreateLock(name);
    }
}