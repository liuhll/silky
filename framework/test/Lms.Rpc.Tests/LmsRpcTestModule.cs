using System;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Rpc.Tests
{
    [DependsOn(typeof(RpcModule))]
    public class LmsRpcTestModule : LmsModule
    {
    }
}