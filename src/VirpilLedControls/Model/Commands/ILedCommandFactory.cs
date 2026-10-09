namespace VirpilLedControls.Model.Commands
{
    public interface ILedCommandFactory
    {
        LedCommand Create(string json);
    }
}