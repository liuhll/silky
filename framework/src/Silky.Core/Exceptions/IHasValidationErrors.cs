using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Core.Exceptions
{
    public interface IHasValidationErrors
    {
        IList<ValidationResult> ValidationErrors { get; }
    }
}