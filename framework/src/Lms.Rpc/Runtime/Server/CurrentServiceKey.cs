using Lms.Core.DependencyInjection;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Runtime.Server
{
    public class CurrentServiceKey : ICurrentServiceKey, IScopedDependency
    {
        public string ServiceKey => RpcContext.GetContext().GetAttachment("serviceKey")?.ToString();

        public void Change(string seviceKey)
        {
            RpcContext.GetContext().SetAttachment("serviceKey", seviceKey);
        }
    }
}