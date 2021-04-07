using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Transaction.Tcc
{
    public static class LmsMethodInvocationExtensions
    {
        public static async Task ExcuteTccMethod(this ILmsMethodInvocation invocation, TccMethodType tccMethodType)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            if (serviceEntry.IsLocal)
            {
                var excutorInfo = serviceEntry.GetTccExcutorInfo(serviceKey, tccMethodType);
                if (excutorInfo.Item2)
                {
                    invocation.ReturnValue = await excutorInfo.Item1.ExecuteAsync(excutorInfo.Item3, parameters);
                }
                else
                {
                    invocation.ReturnValue = excutorInfo.Item1.Execute(excutorInfo.Item3, parameters);
                }
            }
            else
            {
                await invocation.ProceedAsync();
            }
            
        }
    }
}