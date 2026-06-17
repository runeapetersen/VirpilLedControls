using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;

namespace VirpilLedControls
{
    public class SingleColourLightScript : ScriptStub, IScriptAction2
    {
        protected override void InitializeScript()
        {
        }

        protected override void DeinitializeScript()
        {
        }

        protected override string ScriptDataPrefix => nameof(SingleColourLightScript);

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            throw new NotSupportedException("Uses new execution approach");
        }

        public class ParameterDefinition
        {
            public ParameterDefinition(string parameter)
            {
                var match = Regex.Match(parameter, @"^([0-9a-fA-F]{4}\s[0-9a-fA-F]{4})\s([0-9a-fA-F]{2})\s([0-9a-fA-F]{2}\s[0-9a-fA-F]{2}\s[0-9a-fA-F]{2})$");
                if (!match.Success)
                    throw new ArgumentException("Invalid parameter format");
                DeviceId = match.Groups[1].Value;
                ButtonId = match.Groups[2].Value;
                ColorState = match.Groups[3].Value;
                Value = parameter;
            }

            public string Value { get; }

            public string ColorState { get;  }

            public string ButtonId { get;  }

            public string DeviceId { get; }
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
            SharedContext.Instance.Devices.GetOrAddDevice(def.DeviceId).GetOrAddButton(def.ButtonId).UpdateActiveTask(HandleSolidLightCommand, CancellationToken.None, def);
        }

        private void HandleSolidLightCommand(ParameterDefinition def, CancellationToken ct)
        {
            SharedContext.Instance.LaunchToolWithParameters(def.Value, ScriptLogger);
        }

        public int NumberOfParameters => 1;
    }
}