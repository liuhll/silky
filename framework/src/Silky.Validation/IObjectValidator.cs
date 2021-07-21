using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silky.Validation
{
    public interface IObjectValidator
    {
        void Validate(
            object validatingObject,
            string name = null,
            bool allowNull = false
        );

        List<ValidationResult> GetErrors(
            object validatingObject,
            string name = null,
            bool allowNull = false
        );
    }
}