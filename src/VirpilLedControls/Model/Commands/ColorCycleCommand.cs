using System.Text;

namespace VirpilLedControls.Model.Commands
{
    public class ColorCycleCommand : SingleLedCommand
    {
        public LedColor[] Colors { get; set; }
        public uint IntervalMs { get; set; }

        protected override void ValidateInternal(StringBuilder errors)
        {
            if (Colors == null || Colors.Length == 0)
            {
                errors.AppendLine($"Invalid argument. Expected at least one color in property '{nameof(Colors)}'.");
            }

            if (IntervalMs < 250)
            {
                errors.AppendLine(
                    $"Invalid argument. IntervalMs should be at least 250 in property '{nameof(IntervalMs)}'.");
            }
        }
    }
}