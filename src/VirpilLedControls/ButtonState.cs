using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    internal class ButtonState
    {
        private readonly int _configButton;
        private CancellationTokenSource _cts;
        private Task<int> _task;
        private readonly ILogger _logger;
        private readonly ISmartLock _lockObject;
        
        public ButtonState(int configButton, ILogger logger)
        {
            _configButton = configButton;
            _logger = logger.CreateChildLogger(nameof(ButtonState));
            _lockObject = SpadSystem.ApplicationProxy.CreateLock(nameof(ButtonState) + _configButton);
        }

        public void SetColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int? configIntervalMs)
        {
            // NEVER use lock {} as this can deadlock complete SPAD. Always use the buildin SmartLock which will automatically timeout and abort 
            _lockObject.Lock(() =>
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
                        () => ChangeColors(deviceWriter, configColors, configIntervalMs.GetValueOrDefault(), token), token);
                else
                {
                    _task = Task.Factory.StartNew(() => SolidColor(deviceWriter, configColors.First(), token), token);
                }
            });
        }

        private int SolidColor(Action<byte[]> deviceWriter, ButtonColor first, CancellationToken _)
        {
            _logger.Info("Setting solid color for button {Button}", _configButton);
            var packet = PacketHandling.CreatePacket(PacketHandling.BoardType.OnBoard, _configButton, first.R, first.G, first.B);
            try
            {
                deviceWriter(packet);
            }
            catch (Exception e)
            {
                _logger.Error("Error setting solid color for button {Button}: {Error}", _configButton, e);
            }
            return 0;
        }

        private int ChangeColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int configIntervalMs,
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
                    deviceWriter(packet);
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