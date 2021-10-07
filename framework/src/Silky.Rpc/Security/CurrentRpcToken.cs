using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Security
{
    public class CurrentRpcToken : ICurrentRpcToken, IScopedDependency
    {
        private string _token;

        public CurrentRpcToken(IOptionsMonitor<RpcOptions> rpcOptions)
        {
            _token = rpcOptions.CurrentValue.Token;
            rpcOptions.OnChange(options => { _token = rpcOptions.CurrentValue.Token; });
        }

        public string Token { get; } = RpcContext.Context.GetAttachment(AttachmentKeys.RpcToken)?.ToString();

        public void SetRpcToken()
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.RpcToken, _token);
        }
    }
}