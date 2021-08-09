using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public class CurrentServiceKey : ICurrentServiceKey, IScopedDependency
    {
        public string ServiceKey => RpcContext.GetContext().GetAttachment(AttachmentKeys.ServiceKey)?.ToString();

        public void Change(string seviceKey)
        {
            RpcContext.GetContext().SetAttachment(AttachmentKeys.ServiceKey, seviceKey);
        }
    }
}