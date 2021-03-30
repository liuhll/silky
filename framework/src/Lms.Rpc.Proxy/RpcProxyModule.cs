using Lms.Castle;
using Lms.Core.Modularity;
using Lms.Validation;

namespace Lms.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule), typeof(ValidationModule))]
    public class RpcProxyModule : LmsModule
    {
    }
}