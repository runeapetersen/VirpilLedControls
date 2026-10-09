using System.Text;

namespace VirpilLedControls.Model.Commands
{
    public class DoNothingCommand : LedCommand
    {
        protected override void ValidateInternal(StringBuilder errors)
        {
        }
    }
}