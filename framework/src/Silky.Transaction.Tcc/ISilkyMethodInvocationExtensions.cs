using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Parameter;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Tcc
{
    public static class SilkyMethodInvocationExtensions
    {
        public static async Task ExcuteTccMethod(this ISilkyMethodInvocation invocation, TccMethodType tccMethodType)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            if (serviceEntry.IsLocal)
            {
                var (excutor, instance) = serviceEntry.GetTccExcutorInfo(serviceKey, tccMethodType);
                var actualParameters = new List<object>();
                var i = 0;
                if (!serviceEntry.ParameterDescriptors.IsNullOrEmpty())
                {
                    foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
                    {
                        actualParameters.Add(parameterDescriptor.GetActualParameter(parameters[i]));
                    }
                }
                invocation.ReturnValue = await excutor.ExecuteTccMethodAsync(instance, actualParameters.ToArray());
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}