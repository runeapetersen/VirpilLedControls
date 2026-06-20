using System;
using System.Collections.Generic;
using System.Linq;
using HidLibrary;
using System.Text.Json;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;
// ReSharper disable UnusedType.Global

namespace VirpilLedControls
{
    public class VirpilLightAutomationScript : ScriptStub, IScriptAction2, IHasID
    {
        private List<VirpilDevice> _virpilDevices = new List<VirpilDevice>();
        public Guid ID => Guid.Parse("5af37a59-2137-487a-8b1d-94a206d71d89");
        
        protected override void InitializeScript()
        {
            var devs = HidDevices.Enumerate(VirpilDevice.VendorId);
            foreach (var dev in devs)
            {
                ScriptLogger.Info(
                    $"Found device at path: {dev.DevicePath} ProductId: {dev.ProductId} VendorId: {dev.VendorId}");

                if (dev.Capabilities.FeatureReportByteLength > 0)
                {
                    ScriptLogger.Info($"It has features!");
                    _virpilDevices.Add(new VirpilDevice(dev.ProductId, dev.SerialNumber, dev.DevicePath,
                        dev, ScriptLogger));
                }
            }
        }

        protected override void DeinitializeScript()
        {
            // do nothing
        }

        protected override string ScriptDataPrefix => nameof(VirpilLightAutomationScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            throw new NotSupportedException("Unsupported operation. Use IScriptAction2.Execute");
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
                throw new ArgumentException($"Invalid argument. No device found matching ProductId {config.Pid}.");
            }

            device.SetColors(config.Colors, config.Button, config.IntervalMs);
        }

        public int NumberOfParameters => 1;
    }
}