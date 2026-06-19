using System.Collections.Generic;
using HidLibrary;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    internal class ButtonStateContainer
    {
        private readonly object _lock = new object();
        private readonly Dictionary<int, ButtonState> _states = new Dictionary<int, ButtonState>();
        private readonly HidDevice _hidDevice;
        private readonly ILogger _logger;

        public ButtonStateContainer(HidDevice hidDevice, ILogger scriptLogger)
        {
            _hidDevice = hidDevice;
            _logger = scriptLogger.CreateChildLogger(nameof(ButtonStateContainer));
        }

        public void SetColors(IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            lock (_lock)
            {
                _logger.Info("Setting colors for button {Button}", configButton);
                if (_states.TryGetValue(configButton, out var state))
                {
                    state.SetColors(_hidDevice, configColors, configIntervalMs);
                }
                else
                {
                    _logger.Info("Initializing button {Button}", configButton);
                    state = new ButtonState(configButton, _logger);
                    state.SetColors(_hidDevice, configColors, configIntervalMs);
                    _states[configButton] = state;
                }
            }
        }
    }
}