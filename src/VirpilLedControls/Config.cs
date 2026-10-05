using System.Text.Json.Serialization;

namespace VirpilLedControls
{
    public class Config
    {
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Pid { get; set; }
        public uint LedId { get; set; }
        public LedColor[] Colors { get; set; }
        public uint? IntervalMs { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PacketHandling.BoardType BoardType { get; set; } = PacketHandling.BoardType.OnBoard;
    }
}