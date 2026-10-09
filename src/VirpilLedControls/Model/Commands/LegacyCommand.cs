using System;
using System.Text.Json.Serialization;
using VirpilLedControls.DeviceControl;
using VirpilLedControls.SerializationHelpers;

namespace VirpilLedControls.Model.Commands
{
    [Obsolete("Remove in favor of new command classes.")]
    public class LegacyCommand
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