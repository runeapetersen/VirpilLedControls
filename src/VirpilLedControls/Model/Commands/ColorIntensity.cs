using System.Text.Json.Serialization;

namespace VirpilLedControls.Model.Commands
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