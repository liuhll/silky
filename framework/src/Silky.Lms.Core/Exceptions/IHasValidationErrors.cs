using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Lms.Core.Exceptions
{
    public interface IHasValidationErrors
    {
        IList<ValidationResult> ValidationErrors { get; }
    }
}