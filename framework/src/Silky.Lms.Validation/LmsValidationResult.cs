using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Lms.Validation
{
    public class LmsValidationResult : ILmsValidationResult
    {
        public List<ValidationResult> Errors { get; }

        public LmsValidationResult()
        {
            Errors = new List<ValidationResult>();
        }
    }
}