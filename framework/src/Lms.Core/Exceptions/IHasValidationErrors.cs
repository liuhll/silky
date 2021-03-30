using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lms.Core.Exceptions
{
    public interface IHasValidationErrors
    {
        IList<ValidationResult> ValidationErrors { get; }
    }
}