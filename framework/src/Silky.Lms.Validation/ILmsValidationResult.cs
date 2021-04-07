using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Lms.Validation
{
    public interface ILmsValidationResult
    {
        List<ValidationResult> Errors { get; }
    }
}