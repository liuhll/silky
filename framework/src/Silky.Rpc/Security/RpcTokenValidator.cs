using Microsoft.Extensions.Options;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Security
{
    public class RpcTokenValidator : ITokenValidator
    {
        private readonly RpcOptions _rpcOptions;
        private readonly ICurrentRpcToken _currentRpcToken;

        public RpcTokenValidator(IOptions<RpcOptions> rpcOptions,
            ICurrentRpcToken currentRpcToken)
        {
            _currentRpcToken = currentRpcToken;
            _rpcOptions = rpcOptions.Value;
        }

        public bool Validate()
        {
            if (_currentRpcToken.Token.IsNullOrEmpty())
            {
                return false;
            }

            var isEquals = _currentRpcToken.Token.Equals(_rpcOptions.Token);
            return isEquals;
        }
    }
}