using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirpilLedControls
{
    public class Config
    {
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Vid { get; set; }
        [JsonConverter(typeof(HexToDecConverter))]
        public uint Pid { get; set; }
        public int Button { get; set; }
        public IEnumerable<ButtonColor> Colors { get; set; }
        public int? IntervalMs { get; set; }
    }

    public class HexToDecConverter : JsonConverter<uint>
    {
        public override void WriteJson(JsonWriter writer, uint value, JsonSerializer serializer)
        {
            // not super relevant since we don't NEED serialization support
            writer.WriteValue($"0x{value:X}");
        }

        public override uint ReadJson(JsonReader reader, Type objectType, uint existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                throw new JsonSerializationException("Cannot convert null to UInt32");

            string input = reader.Value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                throw new JsonSerializationException("Hex value cannot be empty or whitespace.");

            // Remove optional '0x' or '0X' prefix for flexible input handling
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                input = input.Substring(2);

            try
            {
                return Convert.ToUInt32(input, 16);
            }
            catch (FormatException)
            {
                throw new JsonSerializationException($"Invalid hexadecimal format: '{input}'");
            }
            catch (OverflowException)
            {
                throw new JsonSerializationException($"Hexadecimal value out of range for UInt32: '0x{input}'");
            }
        }
    }
}