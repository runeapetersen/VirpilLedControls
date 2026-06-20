using System;
using System.Collections.Generic;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    internal class ButtonStateContainer
    {
        private readonly ISmartLock _lockObject;
        private readonly Dictionary<int, ButtonState> _states = new Dictionary<int, ButtonState>();
        private readonly ILogger _logger;

        public ButtonStateContainer(ILogger scriptLogger)
        {
            _logger = scriptLogger.CreateChildLogger(nameof(ButtonStateContainer));
            _lockObject = SpadSystem.ApplicationProxy.CreateLock(nameof(ButtonStateContainer));
        }

        public void SetColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            _lockObject.Lock(() =>
            {
                _logger.Info("Setting colors for button {Button}", configButton);
                if (_states.TryGetValue(configButton, out var state))
                {
                    state.SetColors(deviceWriter, configColors, configIntervalMs);
                }
                else
                {
                    _logger.Info("Initializing button {Button}", configButton);
                    state = new ButtonState(configButton, _logger);
                    state.SetColors(deviceWriter, configColors, configIntervalMs);
                    _states[configButton] = state;
                }
            });
        }
    }
}