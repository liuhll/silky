using System.Threading.Tasks;

namespace Silky.Validation
{
    public interface IObjectValidationContributor
    {
        Task AddErrors(ObjectValidationContext context);
    }
}