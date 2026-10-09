using System.Text.Json.Serialization;
using VirpilLedControls.DeviceControl;

namespace VirpilLedControls.Model.Commands
{
    public abstract class SingleLedCommand : LedCommand
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PacketHandling.BoardType BoardType { get; set; }

        public uint LedId { get; set; }
    }
}