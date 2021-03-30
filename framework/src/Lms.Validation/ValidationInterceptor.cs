using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;

namespace Lms.Validation
{
    public class ValidationInterceptor: LmsInterceptor, ITransientDependency
    {
        private readonly IMethodInvocationValidator _methodInvocationValidator;
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
                    invocation.TargetObject,
                    invocation.Method,
                    invocation.Arguments
                )
            );
        }
    }
}