using System;
using System.Linq;
using System.Text.Json;
using VirpilLedControls.Exceptions;
using VirpilLedControls.Model.Commands;

namespace VirpilLedControls.SerializationHelpers
{
    public class LedCommandFactory : ILedCommandFactory
    {
        private const string DiscriminatorName = "Type";

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowOutOfOrderMetadataProperties = true
        };

        public LedCommand Create(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Expected a non-empty JSON string.", nameof(json));

            try
            {
                var command = IsLegacy(json)
                    ? FromLegacy(JsonSerializer.Deserialize<LegacyCommand>(json, Options))
                    : JsonSerializer.Deserialize<LedCommand>(json, Options)
                      ?? throw new ArgumentException("Unable to deserialize JSON configuration.");
                var validationResult = command.Validate();
                if (!validationResult.Successful)
                    throw new ValidationException($"Invalid command. {validationResult.Errors}");
                return command;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON configuration: " + ex.Message, nameof(json), ex);
            }
        }

        private static bool IsLegacy(string json)
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    throw new JsonException("Payload must be a JSON object.");

                return !root.EnumerateObject().Any(p =>
                    string.Equals(p.Name, DiscriminatorName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static LedCommand FromLegacy(LegacyCommand legacy)
        {
            if (legacy == null)
                throw new ArgumentException("Unable to deserialize JSON configuration.");

            var colors = legacy.Colors;
            if (colors == null || colors.Length == 0)
                throw new ArgumentException("Expected at least one color in config.");

            if (colors.Length == 1)
                return new SolidColorCommand
                {
                    Pid = legacy.Pid, BoardType = legacy.BoardType,
                    LedId = legacy.LedId, Color = colors[0]
                };

            if (legacy.IntervalMs == null)
                throw new ArgumentException("IntervalMs is required when cycling more than one color.");

            return new ColorCycleCommand
            {
                Pid = legacy.Pid, BoardType = legacy.BoardType, LedId = legacy.LedId,
                Colors = colors, IntervalMs = legacy.IntervalMs.Value
            };
        }
    }
}