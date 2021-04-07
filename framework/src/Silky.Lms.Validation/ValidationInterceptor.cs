using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.DynamicProxy;

namespace Silky.Lms.Validation
{
    public class ValidationInterceptor: LmsInterceptor, ITransientDependency
    {
        protected readonly IMethodInvocationValidator _methodInvocationValidator;
        public ValidationInterceptor(IMethodInvocationValidator methodInvocationValidator)
        {
            _methodInvocationValidator = methodInvocationValidator;
        }

        public override async Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            Validate(invocation);
            await invocation.ProceedAsync();
        }

        protected virtual void Validate(ILmsMethodInvocation invocation)
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