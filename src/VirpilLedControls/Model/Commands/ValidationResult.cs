namespace VirpilLedControls.Model.Commands
{
    public class ValidationResult
    {
        public bool Successful => string.IsNullOrEmpty(Errors);
        public string Errors { get; set; }
    }
}