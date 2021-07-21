using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;

namespace Silky.Validation
{
    public class ValidationInterceptor: SilkyInterceptor, ITransientDependency
    {
        protected readonly IMethodInvocationValidator _methodInvocationValidator;
        public ValidationInterceptor(IMethodInvocationValidator methodInvocationValidator)
        {
            _methodInvocationValidator = methodInvocationValidator;
        }

        public override async Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            Validate(invocation);
            await invocation.ProceedAsync();
        }

        protected virtual void Validate(ISilkyMethodInvocation invocation)
        {
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(
                    invocation.Method,
                    invocation.Arguments
                )
            );
        }
    }
}