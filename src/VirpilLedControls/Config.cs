using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VirpilLedControls
{
    public class Config
    {
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Vid { get; set; }
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Pid { get; set; }
        public uint LedId { get; set; }
        public LedColor[] Colors { get; set; }
        public uint? IntervalMs { get; set; }
    }
}