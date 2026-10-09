using System.Text;
using System.Text.Json.Serialization;
using VirpilLedControls.SerializationHelpers;

namespace VirpilLedControls.Model.Commands
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(SolidColorCommand), "SingleColor")]
    [JsonDerivedType(typeof(ColorCycleCommand), "ColorCycle")]
    [JsonDerivedType(typeof(SetDeviceSingleColorCommand), "DeviceSingleColor")]
    [JsonDerivedType(typeof(ResetToFirmwareColoursCommand), "DeviceFirmwareDefaultColor")]
    [JsonDerivedType(typeof(DoNothingCommand), "DoNothing")]
    public abstract class LedCommand
    {
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Pid { get; set; }

        public ValidationResult Validate()
        {
            StringBuilder sb = new StringBuilder();
            ValidateInternal(sb);
            return new ValidationResult
            {
                Errors = sb.ToString()
            };
        }

        protected abstract void ValidateInternal(StringBuilder errors);
    }
}