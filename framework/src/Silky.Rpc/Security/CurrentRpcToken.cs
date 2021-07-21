using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Security
{
    public class CurrentRpcToken : ICurrentRpcToken, IScopedDependency
    {
        public string Token { get; } = RpcContext.GetContext().GetAttachment(AttachmentKeys.RpcToken)?.ToString();
    }
}