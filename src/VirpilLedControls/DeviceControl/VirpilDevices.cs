using System;
using System.Collections.Generic;
using System.Linq;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Logging;
using VirpilLedControls.Interfaces;
using VirpilLedControls.Model.Commands;

namespace VirpilLedControls.DeviceControl
{
    public class VirpilDevices : IDisposable
    {
        private readonly ILockFactory _lockFactory;
        private readonly IHidDevices _hidDevices;
        private readonly LedCommand _onDisposeCommand;
        private readonly ISmartLock _lock;
        private readonly ILogger _logger;
        private bool disposed;

        private List<IVirpilDevice> _devices { get; } = new List<IVirpilDevice>();

        public VirpilDevices(ILogger logger, ILockFactory lockFactory, IHidDevices hidDevices,
            LedCommand onDisposeCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lockFactory = lockFactory ?? throw new ArgumentNullException(nameof(lockFactory));
            _hidDevices = hidDevices ?? throw new ArgumentNullException(nameof(hidDevices));
            _onDisposeCommand = onDisposeCommand ?? throw new ArgumentNullException(nameof(onDisposeCommand));
            _lock = lockFactory.CreateLock(nameof(VirpilDevices));
            if (_lock == null) throw new InvalidOperationException("Lock factory returned null lock");
        }

        private IEnumerable<IVirpilDevice> GetByPid(uint pid)
        {
            // Note: might need fast path. Unlikely that performance will be an issue, but if it is, add a dictionary for fast lookup and implement standard double check locking pattern.

            List<IVirpilDevice> res = null;

            res = _devices.Where(d => d.Pid == pid).ToList();
            if (res.Count == 0)
            {
                var hidDevice = _hidDevices.Enumerate(VirpilDevice.VendorId).FirstOrDefault(d =>
                    d.ProductId == pid &&
                    d.Capabilities.FeatureReportByteLength > 0);

                if (hidDevice == null)
                {
                    throw new ArgumentException($"Target device {pid} not found");
                }

                var device = new VirpilDevice(pid, hidDevice, _logger.CreateChildLogger($"VirpilDevice_{pid}"),
                    _lockFactory);
                _devices.Add(device);
                res.Add(device);
            }


            return res;
        }

        // TODO: delegate commands to device. Caller shouldn't have to call GetByPid and then call SetColors on each device. Instead, VirpilDevices should handle that internally.
        public void ExecuteCommand(LedCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _lock.Lock(() =>
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(VirpilDevices));
                var devices = GetByPid(command.Pid);
                foreach (var device in devices)
                {
                    device.SetColors(command);
                }
            });
        }

        public void Dispose()
        {
            var errors = new List<Exception>();

            _lock.Lock(() =>
            {
                if (disposed)
                    return;

                disposed = true;

                foreach (var device in _devices)
                {
                    try
                    {
                        device.SetColors(_onDisposeCommand);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }

                    try
                    {
                        device.Dispose();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }

                try
                {
                    _hidDevices.Dispose();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            });

            if (errors.Count > 0)
                throw new AggregateException("One or more errors occurred while disposing VirpilDevices.", errors);
        }
    }
}