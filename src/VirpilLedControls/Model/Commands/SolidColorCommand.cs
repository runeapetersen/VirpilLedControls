using System.Text;

namespace VirpilLedControls.Model.Commands
{
    public class SolidColorCommand : SingleLedCommand
    {
        public LedColor Color { get; set; }

        protected override void ValidateInternal(StringBuilder errors)
        {
            if (Color == null)
            {
                errors.AppendLine($"Invalid argument. Expected a non-null color in property '{nameof(Color)}'.");
            }
        }
    }
}