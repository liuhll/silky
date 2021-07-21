using System;
using Silky.Core.Modularity;

namespace Silky.Rpc.Tests
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyRpcTestModule : SilkyModule
    {
    }
}