using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VirpilLedControls
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ColorIntensity
    {
        Off,
        Thirty,
        Sixty,
        Full
    }
}