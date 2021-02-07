using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.TestBase.Testing;
using Xunit;

namespace Lms.Rpc.Tests.Runtime.Server.ServiceDiscovery
{
    public class ClrServiceEntryFactory_Test: LmsIntegratedTest<LmsRpcTestModule>
    {
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;

        public ClrServiceEntryFactory_Test()
        {
            _clrServiceEntryFactory = GetRequiredService<IClrServiceEntryFactory>();
        }

        [Fact]
        public void Shuold_CreateServiceEntry()
        {
            
        }
    }
}