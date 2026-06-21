using System.Collections.Generic;
using SPAD.neXt.Interfaces.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.HID;
using VirpilLedControls.Interfaces;

namespace VirpilLedControls
{
    public class VirpilDevice
    {
        public uint Pid { get; }
        internal const int VendorId = 0x3344;

        private readonly ILogger _logger;
        private readonly IHidDevice _hidDevice;
        private readonly ISmartLock _lock;
        private readonly Dictionary<uint, LedColorTask> _activeTasks = new Dictionary<uint, LedColorTask>();

        public VirpilDevice(uint pid, IHidDevice hidDevice, ILogger scriptLogger, ILockFactory lockFactory)
        {
            _logger = scriptLogger.CreateChildLogger(nameof(VirpilDevice));
            _hidDevice = hidDevice;
            Pid = pid;
            _lock = lockFactory.CreateLock(nameof(VirpilDevice));
        }

        public void SetColors(uint ledId, LedColor[] ledColors, uint? configIntervalMs)
        {
            if (ledColors == null) throw new ArgumentNullException(nameof(ledColors));
            if (ledColors.Length == 0) throw new ArgumentException("ledColors cannot be empty", nameof(ledColors));
            
            _lock.Lock(() =>
            {
                _logger.Info($"Setting colors for light {ledId}");

                if (_activeTasks.TryGetValue(ledId, out var existingTask))
                {
                    existingTask.Dispose();
                }

                var newTask = new LedColorTask
                {
                    Cts = new CancellationTokenSource()
                };
                _activeTasks[ledId] = newTask;
                
                newTask.Task = Task.Factory.StartNew(
                    () => ProcessColorSequence(ledId, ledColors, configIntervalMs.GetValueOrDefault(), newTask.Cts.Token),
                    newTask.Cts.Token);
            });
        }

        private void ProcessColorSequence(uint ledId, LedColor[] colors, uint intervalMs, CancellationToken token)
        {
            _logger.Info($"Processing color sequence for Led {ledId}");

            int index = 0;
            while (!token.IsCancellationRequested)
            {
                var color = colors[index];
                var packet = PacketHandling.CreatePacket(PacketHandling.BoardType.OnBoard, ledId, color.R, color.G,
                    color.B);

                try
                {
                    _hidDevice.WriteFeatureData(packet);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error setting color for LED id: {ledId}: {e}");
                }

                if (colors.Length == 1)
                    break;

                try
                {
                    Task.Delay(TimeSpan.FromMilliseconds(intervalMs)).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                index = (index + 1) % colors.Length;
            }
        }

        private class LedColorTask : IDisposable
        {
            public CancellationTokenSource Cts;
            public Task Task;

            public void Dispose()
            {
                Cts.Cancel();
                try
                {
                    Task.Wait();
                }
                catch
                {
                    // Ignore errors during wait as the old task is being replaced
                }

                Cts.Dispose();
            }
        }
    }
}