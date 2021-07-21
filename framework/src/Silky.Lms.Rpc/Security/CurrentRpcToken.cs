using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.Rpc.Security
{
    public class CurrentRpcToken : ICurrentRpcToken, IScopedDependency
    {
        public string Token { get; } = RpcContext.GetContext().GetAttachment(AttachmentKeys.RpcToken)?.ToString();
    }
}