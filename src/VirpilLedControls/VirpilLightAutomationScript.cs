using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Logging;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;
using VirpilLedControls.Interfaces;
using VirpilLedControls.StaticDecoupling;

// ReSharper disable UnusedType.Global

namespace VirpilLedControls
{
    public class VirpilLightAutomationScript : ScriptStub, IScriptAction2, IHasID
    {
        private IScriptLoggerFactory _scriptLoggerFactory = new ScriptLoggerFactory();
        private ILockFactory _lockFactory = new LockFactory();
        private IHidDevices _hidDevices = new HidDevices();
        private VirpilDevices _virpilDevices;
        private ILogger _logger => _scriptLoggerFactory.CreateLogger(nameof(VirpilLightAutomationScript));
        public Guid ID => Guid.Parse("5af37a59-2137-487a-8b1d-94a206d71d89");
        
        public VirpilLightAutomationScript()
        {
            // Likely needed for Reflection activation in SPAD.neXt.
        }
        
        public VirpilLightAutomationScript(IScriptLoggerFactory scriptLoggerFactory, ILockFactory lockFactory, IHidDevices hidDevices) : this()
        {
            _scriptLoggerFactory = scriptLoggerFactory ?? throw new ArgumentNullException(nameof(scriptLoggerFactory));
            _lockFactory = lockFactory ?? throw new ArgumentNullException(nameof(lockFactory));
            _hidDevices = hidDevices ?? throw new ArgumentNullException(nameof(hidDevices));
        }
        
        protected override void InitializeScript()
        {
            _virpilDevices = new VirpilDevices(_scriptLoggerFactory, _lockFactory, _hidDevices);
        }

        protected override void DeinitializeScript()
        {
            _virpilDevices?.Dispose();
        }

        protected override string ScriptDataPrefix => nameof(VirpilLightAutomationScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            // No need to throw. Interface IScriptaction2 ensures this is not called at all.
        }

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            var rawConfigJson = actionParameters?.FirstOrDefault()?.GetValueAs<string>();

            if (string.IsNullOrWhiteSpace(rawConfigJson))
            {
                throw new ArgumentException("Invalid argument. Expected a non-empty JSON string.");
            }
            
            _logger.Info("Received config payload of length {Length}", rawConfigJson.Length);

            var config = JsonSerializer.Deserialize<Config>(rawConfigJson);
            if (config == null)
            {
                throw new ArgumentException("Invalid argument. Unable to deserialize JSON configuration.");
            }
            var colorsList = config.Colors?.ToList();
            if (colorsList == null || !colorsList.Any())
            {
                throw new ArgumentException("Invalid argument. Expected at least one color in config.");
            }
            
            if (colorsList.Count > 1 && config.IntervalMs == null)
            {
                throw new ArgumentException("Invalid argument. IntervalMs is required when cycling more than one color.");
            }
            
            var device = _virpilDevices.GetByPid(config.Pid).FirstOrDefault();
            
            device.SetColors(config.LedId, config.BoardType, config.Colors, config.IntervalMs);
        }

        public int NumberOfParameters => 1;
    }
}