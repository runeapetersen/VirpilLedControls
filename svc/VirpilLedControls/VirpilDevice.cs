using System.Collections.Generic;
using HidLibrary;
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

        public VirpilDevice(ushort pid, string serialNumber, string path, HidDevice hidDevice,
            ILogger scriptLogger)
        {
            _logger = scriptLogger.CreateChildLogger(nameof(VirpilDevice));
            Pid = pid;
            SerialNumber = serialNumber;
            Path = path;
            _buttonStates = new ButtonStateContainer(hidDevice, scriptLogger);
            
        }

        public void SetColors(IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            _buttonStates.SetColors(configColors, configButton, configIntervalMs);
        }
    }
}