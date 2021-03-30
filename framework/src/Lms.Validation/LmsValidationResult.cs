using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lms.Validation
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