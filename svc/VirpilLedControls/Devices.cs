using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VirpilLedControls
{
    public class Devices
    {
        private Dictionary<string, Device> _devices = new Dictionary<string, Device>();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public Device GetOrAddDevice(string id, CancellationToken ct = default)
        {
            _semaphoreSlim.Wait(ct);
            try
            {
                if (_devices.TryGetValue(id, out var device))
                    return device;
                device = new Device(id);
                _devices.Add(device.Id, device);
                return device;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        } 
    }
}