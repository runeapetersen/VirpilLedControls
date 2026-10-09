using System.Text;
using System.Text.Json.Serialization;
using VirpilLedControls.DeviceControl;

namespace VirpilLedControls.Model.Commands
{
    public abstract class SingleLedCommand : LedCommand
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PacketHandling.BoardType BoardType { get; set; }

        public uint LedId { get; set; }

        protected override void ValidateInternal(StringBuilder errors)
        {
            if (LedId < 1 || LedId > 34)
            {
                errors.AppendLine($"Invalid argument. Expected a value between 1 and 34 in property '{nameof(LedId)}'.");
            }
            ValidateInternalSingleLed(errors);
        }
        protected abstract void ValidateInternalSingleLed(StringBuilder errors);
    }
}