using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Logging;
using VirpilLedControls.Interfaces;

namespace VirpilLedControls.StaticDecoupling
{
    public class ScriptLoggerFactory : IScriptLoggerFactory
    {
        public ILogger CreateLogger(string name) => SpadSystem.ApplicationProxy.GetLogger(name);
    }
}