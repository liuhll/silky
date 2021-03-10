using Lms.Castle;
using Lms.Core.Modularity;

namespace Lms.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : LmsModule
    {
        
    }
}