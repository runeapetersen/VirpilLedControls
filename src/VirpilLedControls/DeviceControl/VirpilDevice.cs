using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.HID;
using SPAD.neXt.Interfaces.Logging;
using VirpilLedControls.Interfaces;
using VirpilLedControls.Model.Commands;

namespace VirpilLedControls.DeviceControl
{
    public class VirpilDevice : IVirpilDevice
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
                    int hashcode = 17;
                    hashcode = hashcode * 31 + LedId.GetHashCode();
                    hashcode = hashcode * 31 + ((int)BoardType).GetHashCode();
                    return hashcode;
                }
            }
        }

        public uint Pid { get; }
        public const int VendorId = 0x3344;

        private readonly ILogger _logger;
        private readonly IHidDevice _hidDevice;
        private readonly ISmartLock _lock;

        private readonly Dictionary<CombinedKey, LedColorTask> _activeTasks =
            new Dictionary<CombinedKey, LedColorTask>();

        private bool _isDisposed;

        private LedColorTask _deviceLevelTask;
        //= new LedColorTask{ // placeholder
        //Cts = new CancellationTokenSource(),
        //Logger = null // Will be set when a device-level command is processed.
        //};

        public VirpilDevice(uint pid, IHidDevice hidDevice, ILogger logger, ILockFactory lockFactory)
        {
            Pid = pid;
            _hidDevice = hidDevice ?? throw new ArgumentNullException(nameof(hidDevice));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (lockFactory == null) throw new ArgumentNullException(nameof(lockFactory));
            _lock = lockFactory.CreateLock($"{nameof(VirpilDevice)} Pid {pid}");
            if (_lock == null) throw new InvalidOperationException("Lock factory returned null lock");
        }

        public void SetColors(LedCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _lock.Lock(() =>
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(VirpilDevice));

                switch (command)
                {
                    case DeviceCommand deviceCommand:
                        HandleDeviceLevelCommand(deviceCommand);
                        break;
                    case SingleLedCommand singleLedCommand:
                        HandleSingleLedCommand(singleLedCommand);
                        break;
                    case DoNothingCommand doNothingCommand:
                        // Handle do nothing command
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(command), "Unknown command type.");
                }
            });
        }

        private void HandleSingleLedCommand(SingleLedCommand command)
        {
            LedColor[] colors;
            uint intervalMs;

            switch (command)
            {
                case SolidColorCommand solid:
                    colors = new[] { solid.Color };
                    intervalMs = 0;
                    break;
                case ColorCycleCommand cycle:
                    colors = cycle.Colors;
                    intervalMs = cycle.IntervalMs;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), "Unknown SingleLedCommand type.");
            }

            var key = new CombinedKey(command.LedId, command.BoardType);
            var tasksToStop = new List<LedColorTask>();

            if (_activeTasks.TryGetValue(key, out var previousLedTask))
            {
                _activeTasks.Remove(key);
                tasksToStop.Add(previousLedTask);
            }

            if (_deviceLevelTask != null)
            {
                tasksToStop.Add(_deviceLevelTask);
                _deviceLevelTask = null;
            }

            StopTasks(tasksToStop);

            var newTask = new LedColorTask();
            _activeTasks.Add(key, newTask);
            newTask.Task = Task.Factory.StartNew(
                () => ProcessColorTask(command.LedId, command.BoardType, colors, intervalMs, newTask.Token),
                newTask.Token);
        }


        private void HandleDeviceLevelCommand(DeviceCommand command)
        {
            LedColor[] colors;

            switch (command)
            {
                case SetDeviceSingleColorCommand solid:
                    colors = new[] { solid.Color };
                    break;
                case ResetToFirmwareColoursCommand _:
                    colors = new[]
                    {
                        new LedColor
                        {
                            R = ColorIntensity.Off,
                            G = ColorIntensity.Off,
                            B = ColorIntensity.Off
                        }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), "Unknown DeviceCommand type.");
            }

            var tasksToStop = new List<LedColorTask>(_activeTasks.Values);
            _activeTasks.Clear();

            if (_deviceLevelTask != null)
            {
                tasksToStop.Add(_deviceLevelTask);
                _deviceLevelTask = null;
            }

            StopTasks(tasksToStop);

            _deviceLevelTask = new LedColorTask();
            _deviceLevelTask.Task = Task.Factory.StartNew(
                () => ProcessColorTask(1, command.BoardType, colors, 0, _deviceLevelTask.Token),
                _deviceLevelTask.Token);
        }

        private void ProcessColorTask(uint ledId, PacketHandling.BoardType boardType, LedColor[] colors,
            uint intervalMs, CancellationToken token)
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
                catch (UnauthorizedAccessException ex)
                {
                    _logger.Error($"Access denied when setting color for LED id: {ledId}: {ex}");
                    break;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error setting color for LED id: {ledId}: {e}");
                    // ignore possible transient errors and continue with the next color in the sequence
                }

                if (colors.Length == 1)
                    break;

                try
                {
                    Task.Delay(TimeSpan.FromMilliseconds(intervalMs), token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    _logger.Info($"Color sequence processing cancelled for LED {ledId}");
                    break;
                }

                index = (index + 1) % colors.Length;
            }
        }

        private sealed class LedColorTask : IDisposable
        {
            private readonly object _disposeLock = new object();

            private readonly CancellationTokenSource _cancellationTokenSource =
                new CancellationTokenSource();

            private bool _disposed;

            public CancellationToken Token => _cancellationTokenSource.Token;
            public Task Task { get; set; }

            public void Dispose()
            {
                lock (_disposeLock)
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    var errors = new List<Exception>();

                    try
                    {
                        try
                        {
                            _cancellationTokenSource.Cancel();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex);
                        }

                        try
                        {
                            Task?.GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex);
                        }
                    }
                    finally
                    {
                        try
                        {
                            _cancellationTokenSource.Dispose();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex);
                        }
                    }

                    if (errors.Count > 0)
                        throw new AggregateException("Failed to stop the color task cleanly.", errors);
                }
            }
        }

        public void Dispose()
        {
            var errors = new List<Exception>();

            _lock.Lock(() =>
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                var tasks = new List<LedColorTask>(_activeTasks.Values);
                _activeTasks.Clear();

                if (_deviceLevelTask != null)
                {
                    tasks.Add(_deviceLevelTask);
                    _deviceLevelTask = null;
                }

                try
                {
                    StopTasks(tasks);
                }
                catch (AggregateException ex)
                {
                    errors.AddRange(ex.InnerExceptions);
                }

                try
                {
                    _hidDevice.Dispose();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            });

            if (errors.Count > 0)
                throw new AggregateException("Errors occurred while disposing the device.", errors);
        }

        private static void StopTasks(IEnumerable<LedColorTask> tasks)
        {
            var errors = new List<Exception>();
            foreach (var task in tasks)
            {
                try
                {
                    task.Dispose();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            if (errors.Count > 0)
                throw new AggregateException("One or more color tasks failed to stop.", errors);
        }
    }
}