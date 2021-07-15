using System;
using System.Diagnostics;
using Silky.Lms.Core;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Validation;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Interceptors
{
    public class RpcValidationInterceptor : ValidationInterceptor
    {
        public RpcValidationInterceptor(IMethodInvocationValidator methodInvocationValidator)
            : base(methodInvocationValidator)
        {
        }

        protected override void Validate(ILmsMethodInvocation invocation)
        {
            EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.ValidationInterceptor.Name,
                MiniProfileConstant.ValidationInterceptor.State.Begin,
                $"开始校验输入参数");
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
                EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Success,
                    $"Success");
            }
            catch (Exception e)
            {
                EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Fail,
                    $"输入参数校验失败", true);
                throw;
            }
        }
    }
}