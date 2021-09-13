using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public class CurrentServiceKey : ICurrentServiceKey, IScopedDependency
    {
        public string ServiceKey => RpcContext.Context.GetAttachment(AttachmentKeys.ServiceKey)?.ToString();

        public void Change(string serviceKey)
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.ServiceKey, serviceKey);
        }
    }
}