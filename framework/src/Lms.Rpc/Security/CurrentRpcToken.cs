using Lms.Core.DependencyInjection;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Security
{
    public class CurrentRpcToken : ICurrentRpcToken, IScopedDependency
    {
        public string Token { get; } = RpcContext.GetContext().GetAttachment("token")?.ToString();
    }
}