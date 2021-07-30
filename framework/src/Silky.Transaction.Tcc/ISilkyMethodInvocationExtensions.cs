using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Tcc
{
    public static class SilkyMethodInvocationExtensions
    {
        public static async Task ExcuteTccMethod(this ISilkyMethodInvocation invocation, TccMethodType tccMethodType,
            TransactionContext context)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            if (serviceEntry.IsLocal)
            {
                var (excutor, instance) = serviceEntry.GetTccExcutorInfo(serviceKey, tccMethodType);
                invocation.ReturnValue = await excutor.ExecuteTccMethodAsync(instance, parameters);
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}