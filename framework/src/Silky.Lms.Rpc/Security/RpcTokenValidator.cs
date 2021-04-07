using Microsoft.Extensions.Options;
using Silky.Lms.Rpc.Configuration;

namespace Silky.Lms.Rpc.Security
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
            return _currentRpcToken.Token == _rpcOptions.Token;
        }
    }
}