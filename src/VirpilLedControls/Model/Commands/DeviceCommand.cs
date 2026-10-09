using VirpilLedControls.DeviceControl;

namespace VirpilLedControls.Model.Commands
{
    public abstract class DeviceCommand : LedCommand
    {
        public abstract PacketHandling.BoardType BoardType { get; }
    }
}