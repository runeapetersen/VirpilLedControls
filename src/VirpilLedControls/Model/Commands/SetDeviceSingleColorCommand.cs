using System.Text;
using VirpilLedControls.DeviceControl;

namespace VirpilLedControls.Model.Commands
{
    public class SetDeviceSingleColorCommand : DeviceCommand
    {
        public override PacketHandling.BoardType BoardType => PacketHandling.BoardType.ResetToColorReserved;
        public LedColor Color { get; set; }

        protected override void ValidateInternal(StringBuilder errors)
        {
            if (Color == null)
            {
                errors.AppendLine($"Invalid argument. Expected a non-null color in property '{nameof(Color)}'.");
            }
        }
    }
}