using System;
using System.Collections.Generic;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    public class VirpilDevice
    {
        public ushort Pid { get; }
        internal const int VendorId = 0x3344;

        private readonly ButtonStateContainer _buttonStates;
        private readonly ILogger _logger;
                
        // We assume only one device is present
        public string SpadDeviceProfileID => $"{VendorId:4X}:{Pid:4X}:0";
        public VirpilDevice(ushort pid, ILogger scriptLogger)
        {
            _logger = scriptLogger.CreateChildLogger(nameof(VirpilDevice));
            Pid = pid;
            _buttonStates = new ButtonStateContainer(scriptLogger);
            
        }

        public void SetColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            _buttonStates.SetColors(deviceWriter,configColors, configButton, configIntervalMs);
        }
    }
}