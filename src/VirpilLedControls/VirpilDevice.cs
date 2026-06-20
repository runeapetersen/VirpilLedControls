using System;
using System.Collections.Generic;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    public class VirpilDevice
    {
        public ushort Pid { get; }
        public string SerialNumber { get; }
        public string Path { get; }
        internal const int VendorId = 0x3344;

        private readonly ButtonStateContainer _buttonStates;
        private readonly ILogger _logger;

        public VirpilDevice(ushort pid, string serialNumber, string path, ILogger scriptLogger)
        {
            _logger = scriptLogger.CreateChildLogger(nameof(VirpilDevice));
            Pid = pid;
            SerialNumber = serialNumber;
            Path = path;
            _buttonStates = new ButtonStateContainer(scriptLogger);
            
        }

        public void SetColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            _buttonStates.SetColors(deviceWriter,configColors, configButton, configIntervalMs);
        }
    }
}