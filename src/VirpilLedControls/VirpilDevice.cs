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
    public class VirpilDevice : IDisposable
    {
        private class CombinedKey
        {
            public uint LedId { get; }
            public PacketHandling.BoardType BoardType { get; }

            public CombinedKey(uint ledId, PacketHandling.BoardType boardType)
            {
                LedId = ledId;
                BoardType = boardType;
            }

            public override bool Equals(object obj)
            {
                if (obj is CombinedKey other)
                {
                    return LedId == other.LedId && BoardType == other.BoardType;
                }
                return false;
            }

            public override int GetHashCode()
            {
                unchecked // Allow arithmetic overflow, numbers will just "wrap around"
                {
                    int hashcode = 1430287;
                    hashcode = hashcode * 7302013 ^ LedId.GetHashCode();
                    return hashcode * 7302013 ^ ((byte)BoardType).GetHashCode();
                }
            }
        }
        
        public uint Pid { get; }
        public const int VendorId = 0x3344;

        private readonly ILogger _logger;
        private readonly IHidDevice _hidDevice;
        private readonly ISmartLock _lock;
        private readonly Dictionary<CombinedKey, LedColorTask> _activeTasks = new Dictionary<CombinedKey, LedColorTask>();

        public VirpilDevice(uint pid, IHidDevice hidDevice, IScriptLoggerFactory scriptLoggerFactory, ILockFactory lockFactory)
        {
            _logger = scriptLoggerFactory.CreateLogger(nameof(VirpilDevice));
            _lock = lockFactory.CreateLock(nameof(VirpilDevice));
            _hidDevice = hidDevice;
            Pid = pid;
        }

        public void SetColors(uint ledId, PacketHandling.BoardType boardType, LedColor[] ledColors, uint? configIntervalMs)
        {
            if (ledColors == null) throw new ArgumentNullException(nameof(ledColors));
            if (ledColors.Length == 0) throw new ArgumentException("ledColors cannot be empty", nameof(ledColors));
            if (ledColors.Length > 1)
            {
                if (configIntervalMs == null)
                    throw new ArgumentException("configIntervalMs is required when cycling more than one color.", nameof(configIntervalMs));
                if (configIntervalMs < 250)
                    throw new ArgumentException("configIntervalMs must be greater than 250ms.", nameof(configIntervalMs));
            }
            
            _lock.Lock(() =>
            {
                _logger.Info($"Setting colors for light {ledId}");

                if (_activeTasks.TryGetValue(new CombinedKey(ledId, boardType), out var existingTask))
                {
                    existingTask.Dispose();
                }

                var newTask = new LedColorTask
                {
                    Cts = new CancellationTokenSource()
                };
                _activeTasks[new CombinedKey(ledId, boardType)] = newTask;
                
                newTask.Task = Task.Factory.StartNew(
                    () => ProcessColorSequence(ledId,boardType, ledColors, configIntervalMs.GetValueOrDefault(), newTask.Cts.Token),
                    newTask.Cts.Token);
            });
        }

        private void ProcessColorSequence(uint ledId, PacketHandling.BoardType boardType, LedColor[] colors, uint intervalMs, CancellationToken token)
        {
            _logger.Info($"Processing color sequence for Led {ledId}");

            int index = 0;
            while (!token.IsCancellationRequested)
            {
                var color = colors[index];
                var packet = PacketHandling.CreatePacket(boardType, ledId, color.R, color.G,
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

        public void Dispose()
        {
            _lock.Lock(() =>
            {
                foreach (var task in _activeTasks.Values)
                {
                    task.Dispose();
                }
                _activeTasks.Clear();
                _hidDevice?.Dispose();
            });
        }
    }
}