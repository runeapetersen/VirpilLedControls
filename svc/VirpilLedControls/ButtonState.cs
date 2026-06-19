using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    internal class ButtonState
    {
        private readonly int _configButton;
        private CancellationTokenSource _cts;
        private Task<int> _task;
        private readonly ILogger _logger;
        private readonly object _lockObject = new object();
        
        public ButtonState(int configButton, ILogger logger)
        {
            _configButton = configButton;
            _logger = logger.CreateChildLogger(nameof(ButtonState));
        }

        public void SetColors(HidDevice device, IEnumerable<ButtonColor> configColors, int? configIntervalMs)
        {
            lock (_lockObject)
            {
                _cts?.Cancel();
                try
                {
                    _task?.Wait();
                }
                catch
                {
                }

                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                var colorCount = configColors.Count();

                if (colorCount > 1)
                    _task = Task.Factory.StartNew(
                        () => ChangeColors(device, configColors, configIntervalMs.GetValueOrDefault(), token), token);
                else
                {
                    _task = Task.Factory.StartNew(() => SolidColor(device, configColors.First(), token), token);
                }
            }
        }

        private int SolidColor(HidDevice device, ButtonColor first, CancellationToken _)
        {
            _logger.Info("Setting solid color for button {Button}", _configButton);
            var packet = PacketHandling.CreatePacket(PacketHandling.BoardType.OnBoard, _configButton, first.R, first.G, first.B);
            try
            {
                device.WriteFeatureData(packet);
            }
            catch (Exception e)
            {
                _logger.Error("Error setting solid color for button {Button}: {Error}", _configButton, e);
            }
            return 0;
        }

        private int ChangeColors(HidDevice device, IEnumerable<ButtonColor> configColors, int configIntervalMs,
            CancellationToken token)
        {
            var colors = configColors.ToArray();
            _logger.Info("Changing colors for button {Button}", _configButton);
            int i = 0;
            while (!token.IsCancellationRequested)
            {
                var packet = PacketHandling.CreatePacket(PacketHandling.BoardType.OnBoard, _configButton, colors[i].R, colors[i].G, colors[i].B);
                try
                {
                    device.WriteFeatureData(packet);
                }
                catch (Exception e)
                {
                    _logger.Error("Error setting color for button {Button}: {Error}", _configButton, e);
                }

                i = (i + 1) % colors.Length;
                try
                {
                    Task.Delay(TimeSpan.FromMilliseconds(configIntervalMs)).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return 0;
        }
    }
}