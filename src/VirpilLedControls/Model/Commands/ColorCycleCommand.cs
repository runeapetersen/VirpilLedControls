using System.Text;

namespace VirpilLedControls.Model.Commands
{
    public class ColorCycleCommand : SingleLedCommand
    {
        public LedColor[] Colors { get; set; }
        public uint IntervalMs { get; set; }

        protected override void ValidateInternalSingleLed(StringBuilder errors)
        {
            if (Colors == null || Colors.Length == 0)
            {
                errors.AppendLine($"Invalid argument. Expected at least one color in property '{nameof(Colors)}'.");
            }
            else if (System.Array.Exists(Colors, color => color == null))
            {
                errors.AppendLine($"Invalid argument. Expected every color in property '{nameof(Colors)}' to be non-null.");
            }
            if (IntervalMs < 250)
            {
                errors.AppendLine(
                    $"Invalid argument. IntervalMs should be at least 250 in property '{nameof(IntervalMs)}'.");
            }
        }
    }
}