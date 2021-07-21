using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Validation
{
    public class SilkyValidationResult : ISilkyValidationResult
    {
        public List<ValidationResult> Errors { get; }

        public SilkyValidationResult()
        {
            Errors = new List<ValidationResult>();
        }
    }
}