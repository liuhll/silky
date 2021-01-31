using Lms.Core;
using Lms.Rpc.Address;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Utils
{
    public static class NetUtil
    {
        private static IAddressModel _host = null;

        public static IAddressModel GetHostAddress()
        {
            var rpcOptions = EngineContext.Current.Resolve<IOptions<RpcOptions>>().Value;
            return null;
        }
    }
}