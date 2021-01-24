using System;
using Lms.Core.Modularity;

namespace Lms.Rpc.Tests
{
    [DependsOn(typeof(RpcModule))]
    public class LmsRpcTestModule : LmsModule
    {
    }
}