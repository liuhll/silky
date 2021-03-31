using Lms.Castle;
using Lms.Core.Modularity;
using Lms.Validation;

namespace Lms.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : LmsModule
    {
    }
}