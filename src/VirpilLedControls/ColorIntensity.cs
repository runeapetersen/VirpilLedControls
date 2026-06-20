using System.Text.Json.Serialization;

namespace VirpilLedControls
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ColorIntensity
    {
        Off,
        Thirty,
        Sixty,
        Full
    }
}