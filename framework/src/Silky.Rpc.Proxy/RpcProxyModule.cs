using Silky.Castle;
using Silky.Core.Modularity;

namespace Silky.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : SilkyModule
    {
    }
}