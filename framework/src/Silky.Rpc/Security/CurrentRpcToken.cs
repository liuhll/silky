using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Security
{
    public class CurrentRpcToken : ICurrentRpcToken, ITransientDependency
    {
        private string _token;

        public CurrentRpcToken(IOptions<RpcOptions> rpcOptions)
        {
            _token = rpcOptions.Value.Token;
        }

        public string? Token { get; } = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.RpcToken);

        public void SetRpcToken()
        {
            if (Token.IsNullOrEmpty())
            {
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RpcToken, _token);
            }
        }
    }
}