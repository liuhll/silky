using System.Diagnostics;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;
using Lms.Validation;

namespace Lms.Rpc.Interceptors
{
    public class RpcValidationInterceptor : ValidationInterceptor
    {
        public RpcValidationInterceptor(IMethodInvocationValidator methodInvocationValidator) 
            : base(methodInvocationValidator)
        {
        }

        protected override void Validate(ILmsMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            parameters = serviceEntry.ConvertParameters(parameters);
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(
                    serviceEntry.MethodInfo,
                    parameters
                )
            );
        }
    }
}