using System.Text;
using VirpilLedControls.DeviceControl;

namespace VirpilLedControls.Model.Commands
{
    public class ResetToFirmwareColoursCommand : DeviceCommand
    {
        public override PacketHandling.BoardType BoardType => PacketHandling.BoardType.ResetToDefaultsReserved;

        protected override void ValidateInternal(StringBuilder errors)
        {
        }
    }
}