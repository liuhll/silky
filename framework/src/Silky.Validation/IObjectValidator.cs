using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Silky.Validation
{
    public interface IObjectValidator
    {
        Task Validate(
            object validatingObject,
            string name = null,
            bool allowNull = false
        );

        Task<List<ValidationResult>> GetErrors(
            object validatingObject,
            string name = null,
            bool allowNull = false
        );
    }
}