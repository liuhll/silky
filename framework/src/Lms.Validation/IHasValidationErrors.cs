using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lms.Validation
{
    public interface IHasValidationErrors
    {
        IList<ValidationResult> ValidationErrors { get; }
    }
}