using System.Threading.Tasks;

namespace Silky.Validation
{
    public interface IMethodInvocationValidator
    {
        Task Validate(MethodInvocationValidationContext context);
    }
}