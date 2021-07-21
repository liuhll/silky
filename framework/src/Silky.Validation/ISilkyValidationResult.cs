using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Validation
{
    public interface ISilkyValidationResult
    {
        List<ValidationResult> Errors { get; }
    }
}