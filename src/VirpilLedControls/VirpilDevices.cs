using System;
using System.Collections.Generic;
using System.Linq;
using SPAD.neXt.Interfaces;
using VirpilLedControls.Interfaces;

namespace VirpilLedControls
{
    public class VirpilDevices : IDisposable
    {
        private readonly IScriptLoggerFactory _scriptLoggerFactory;
        private readonly ILockFactory _lockFactory;
        private readonly IHidDevices _hidDevices;
        private readonly ISmartLock _lock;
        
        private List<VirpilDevice> _devices { get; } = new List<VirpilDevice>();
        
        public VirpilDevices(IScriptLoggerFactory loggerFactory, ILockFactory lockFactory, IHidDevices hidDevices)
        {
            _scriptLoggerFactory = loggerFactory;
            _lockFactory = lockFactory;
            _hidDevices = hidDevices;
            _lock = lockFactory.CreateLock(nameof(VirpilDevices));
        }
        
        public IEnumerable<VirpilDevice> GetByPid(uint pid)
        {
            // Note: might need fast path. Unlikely that performance will be an issue, but if it is, add a dictionary for fast lookup and implement standard double check locking pattern.
            
            List<VirpilDevice> res = null;
            _lock.Lock(() =>
            {
                res = _devices.Where(d => d.Pid == pid).ToList();
                if (res.Count == 0)
                {
                    var hidDevice = _hidDevices.Enumerate(VirpilDevice.VendorId).FirstOrDefault(d =>
                        d.ProductId == pid &&
                        d.Capabilities.FeatureReportByteLength > 0);

                    if (hidDevice == null)
                    {
                        throw new ArgumentException($"Targetdevice {pid} not found");
                    }
                    var device = new VirpilDevice(pid, hidDevice, _scriptLoggerFactory, _lockFactory);
                    _devices.Add(device);
                    res.Add(device);
                }
            });

            return res;
        }

        public void Dispose()
        {
            foreach (var device in _devices)
            {
                device.Dispose();
            }
        }
    }
}