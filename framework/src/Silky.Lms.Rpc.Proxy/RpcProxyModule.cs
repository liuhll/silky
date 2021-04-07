using Silky.Lms.Castle;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : LmsModule
    {
    }
}