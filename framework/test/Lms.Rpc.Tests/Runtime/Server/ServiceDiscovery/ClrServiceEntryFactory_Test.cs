using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;
using Lms.Rpc.Tests.AppService;
using Lms.TestBase.Testing;
using Shouldly;
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
            _clrServiceEntryFactory.CreateServiceEntry(typeof(ITestAppService)).ShouldNotBeEmpty();
        }
    }
}