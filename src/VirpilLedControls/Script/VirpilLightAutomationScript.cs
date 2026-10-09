using System;
using System.Collections.Generic;
using System.Linq;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Logging;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;
using VirpilLedControls.DeviceControl;
using VirpilLedControls.Interfaces;
using VirpilLedControls.Model.Commands;
using VirpilLedControls.SerializationHelpers;
using VirpilLedControls.StaticDecoupling;

// ReSharper disable UnusedType.Global

namespace VirpilLedControls.Script
{
    public class VirpilLightAutomationScript : ScriptStub, IScriptAction2, IHasID
    {
        private readonly ILockFactory _lockFactory;
        private readonly IHidDevices _hidDevices;
        private readonly ILedCommandFactory _ledCommandFactory;
        private readonly ILogger _logger;

        private VirpilDevices _virpilDevices;

        public Guid ID => Guid.Parse("5af37a59-2137-487a-8b1d-94a206d71d89");

        // This constructor is used by the SPAD.neXt scripting engine to instantiate the script.
        // ReSharper disable once UnusedMember.Global
        public VirpilLightAutomationScript()
            : this(new ScriptLoggerFactory(),
                new LockFactory(),
                new HidDevices(),
                new LedCommandFactory())
        {
        }

        protected VirpilLightAutomationScript(IScriptLoggerFactory scriptLoggerFactory, ILockFactory lockFactory,
            IHidDevices hidDevices, ILedCommandFactory ledCommandFactory)
        {
            _logger = scriptLoggerFactory?.CreateLogger(nameof(VirpilLightAutomationScript))
                      ?? throw new ArgumentNullException(nameof(scriptLoggerFactory));
            _lockFactory = lockFactory ?? throw new ArgumentNullException(nameof(lockFactory));
            _hidDevices = hidDevices ?? throw new ArgumentNullException(nameof(hidDevices));
            _ledCommandFactory = ledCommandFactory ?? throw new ArgumentNullException(nameof(ledCommandFactory));
        }

        protected override void InitializeScript()
        {
            _virpilDevices = new VirpilDevices(_logger.CreateChildLogger(nameof(VirpilDevices)), _lockFactory,
                _hidDevices, new ResetToFirmwareColoursCommand());
        }

        protected override void DeinitializeScript()
        {
            _virpilDevices?.Dispose();
        }

        protected override string ScriptDataPrefix => nameof(VirpilLightAutomationScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            // Interface IScriptaction2 ensures this is not called at all.
        }

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            var rawConfigJson = actionParameters?.FirstOrDefault()?.GetValueAs<string>();

            if (string.IsNullOrWhiteSpace(rawConfigJson))
            {
                throw new ArgumentException("Invalid argument. Expected a non-empty JSON string.");
            }

            _logger.Info("Received config payload of length {Length}", rawConfigJson.Length);

            LedCommand command = _ledCommandFactory.Create(rawConfigJson);

            _virpilDevices.ExecuteCommand(command);
        }

        public int NumberOfParameters => 1;
    }
}