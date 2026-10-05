using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls.Interfaces
{
    public interface IScriptLoggerFactory
    {
        ILogger CreateLogger(string name);
    }
}