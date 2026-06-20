using System.Collections.Generic;
using SPAD.neXt.Interfaces.Logging;
using System;

namespace VirpilLedControls
{
    public class VirpilDevice
    {
        public uint Pid { get; }
        internal const int VendorId = 0x3344;
        private readonly ButtonStateContainer _buttonStates;

        public VirpilDevice(uint pid, ILogger scriptLogger) // note to self: pid might be too simple for filtering if multiple control panels of the same type are present. Sticking with it for now - it's an edge case.
        {
            scriptLogger = scriptLogger.CreateChildLogger(nameof(VirpilDevice));
            Pid = pid;
            _buttonStates = new ButtonStateContainer(scriptLogger);
        }

        public void SetColors(Action<byte[]> deviceWriter, IEnumerable<ButtonColor> configColors, int configButton, int? configIntervalMs)
        {
            _buttonStates.SetColors(deviceWriter,configColors, configButton, configIntervalMs);
        }
    }
}