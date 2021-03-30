using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lms.Validation
{
    public interface ILmsValidationResult
    {
        List<ValidationResult> Errors { get; }
    }
}