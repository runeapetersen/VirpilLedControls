using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;

namespace VirpilLedControls
{
    public class BlinkingLightScript : ScriptStub, IScriptAction2
    {
        protected override void InitializeScript()
        {
        }

        protected override void DeinitializeScript()
        {
        }

        protected override string ScriptDataPrefix => nameof(BlinkingLightScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            throw new NotSupportedException("Uses new execution approach");
        }

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            if (actionParameters.Count()!=1)
                throw new ArgumentException("Only accepts one parameter!");
            var parameter = actionParameters[0];
            if (!parameter.HasParameterValue)
                throw new ArgumentException("Parameter must have a value");
            var value = parameter.GetParameterValue<string>();
            ParameterDefinition def = new ParameterDefinition(value);
            SharedContext.Instance.Devices.GetOrAddDevice(def.DeviceId).GetOrAddButton(def.ButtonId).UpdateActiveTask(ExecuteBlinkingLightLoop, CancellationToken.None, def);
        }

        private void ExecuteBlinkingLightLoop(ParameterDefinition parameters, CancellationToken ct)
        {
            int i = 0;
            var colors = new[] { parameters.FirstColorState, parameters.SecondColorState };
            while (!ct.IsCancellationRequested)
            {
                SharedContext.Instance.LaunchToolWithParameters($"{parameters.DeviceId} {parameters.ButtonId} {colors[i]}", base.ScriptLogger);
                i = ++i % 2;

                try
                {
                    Task.Delay(TimeSpan.FromMilliseconds(parameters.BlinkIntervalMs)).Wait(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        public int NumberOfParameters => 1;

        public class ParameterDefinition
        {
            public ParameterDefinition(string parameter)
            {
                var match = Regex.Match(parameter, @"^([0-9a-fA-F]{4}\s[0-9a-fA-F]{4})\s([0-9a-fA-F]{2})\s([0-9a-fA-F]{2}\s[0-9a-fA-F]{2}\s[0-9a-fA-F]{2})\s([0-9a-fA-F]{2}\s[0-9a-fA-F]{2}\s[0-9a-fA-F]{2})\s([0-9]{3,5})$");
                if (!match.Success)
                    throw new ArgumentException("Invalid parameter format");
                // deviceId is the first two occurrences in the parameter string
                DeviceId = match.Groups[1].Value;
                // buttonId is the third occurrence in the parameter string
                ButtonId = match.Groups[2].Value;
                // colorstate is the remaining three occurrences in the parameter string
                FirstColorState = match.Groups[3].Value;
                SecondColorState = match.Groups[4].Value;
                BlinkIntervalMs = int.Parse(match.Groups[5].Value);
            }

            public string DeviceId { get;  }
            public string ButtonId { get;  }
            public string FirstColorState { get; }
            public string SecondColorState { get; }
            public int BlinkIntervalMs { get; }
        }
    }
}