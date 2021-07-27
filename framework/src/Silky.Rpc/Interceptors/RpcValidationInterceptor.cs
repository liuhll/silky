using System;
using System.Diagnostics;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.MiniProfiler;
using Silky.Validation;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Interceptors
{
    public class RpcValidationInterceptor : ValidationInterceptor
    {
        public RpcValidationInterceptor(IMethodInvocationValidator methodInvocationValidator)
            : base(methodInvocationValidator)
        {
        }

        protected override void Validate(ISilkyMethodInvocation invocation)
        {
            try
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
                MiniProfilerPrinter.Print(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Success,
                    $"Input parameter verification succeeded");
            }
            catch (Exception e)
            {
                MiniProfilerPrinter.Print(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Fail,
                    $"Input parameter is invalid", true);
                throw;
            }
        }
    }
}