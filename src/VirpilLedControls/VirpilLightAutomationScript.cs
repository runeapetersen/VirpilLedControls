using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;
using SPAD.neXt.Interfaces.Configuration;
// ReSharper disable UnusedType.Global

namespace VirpilLedControls
{
    public class VirpilLightAutomationScript : ScriptStub, IScriptAction2, IHasID
    {
        private List<VirpilDevice> _virpilDevices = new List<VirpilDevice>();
        public Guid ID => Guid.Parse("5af37a59-2137-487a-8b1d-94a206d71d89");
        
        protected override void InitializeScript()
        {          
        }

        protected override void DeinitializeScript()
        {
            // do nothing
        }

        protected override string ScriptDataPrefix => nameof(VirpilLightAutomationScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            // No need to throw. Interface IScriptaction2 ensures this is not called at all.
            // throw new NotSupportedException("Unsupported operation. Use IScriptAction2.Execute");
        }

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            var rawConfigJson = actionParameters?.FirstOrDefault()?.GetValueAs<string>();

            if (string.IsNullOrWhiteSpace(rawConfigJson))
            {
                throw new ArgumentException("Invalid argument. Expected a non-empty JSON string.");
            }
            
            ScriptLogger.Info("Received config payload of length {Length}", rawConfigJson.Length);

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
            
            var device = _virpilDevices.FirstOrDefault(d => d.Pid == config.Pid);
            if (device == null)
            {                
                device = new VirpilDevice((ushort)(config.Pid & 0xFFFFu), ScriptLogger);
                _virpilDevices.Add(device);                
            }

            var deviceProfile = app.ActiveProfile.Devices.FirstOrDefault(d => d.DeviceProfileID == device.SpadDeviceProfileID);
            if (deviceProfile == null)
            {
                throw new ArgumentException($"Targetdevice {device.SpadDeviceProfileID} not found");
            }
            if (deviceProfile.InputDevice is IPanelDevice panelDevice)
            {
                device.SetColors((data) => panelDevice.WriteFeatureData(data), config.Colors, config.Button, config.IntervalMs);
            }
            else
            {
                throw new ArgumentException($"Device {device.SpadDeviceProfileID} has no attached hidDevice");
            }
        
        }

        public int NumberOfParameters => 1;
    }
}