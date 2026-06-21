using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirpilLedControls
{
    public class HexToDecConverter : JsonConverter<uint>
    {
        public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a string token for hexadecimal conversion, got {reader.TokenType}.");

            string input = reader.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                throw new JsonException("Hex value cannot be empty or whitespace.");

            // Remove optional '0x' or '0X' prefix for flexible input handling
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                input = input.Substring(2);

            try
            {
                return Convert.ToUInt32(input, 16);
            }
            catch (FormatException)
            {
                throw new JsonException($"Invalid hexadecimal format: '{input}'");
            }
            catch (OverflowException)
            {
                throw new JsonException($"Hexadecimal value out of range for UInt32: '0x{input}'");
            }
        }

        public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"0x{value:X}");
        }
    }
}