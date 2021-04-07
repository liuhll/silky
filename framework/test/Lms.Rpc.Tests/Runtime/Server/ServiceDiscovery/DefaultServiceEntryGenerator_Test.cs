using Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Lms.TestBase.Testing;
using Xunit;

namespace Silky.Lms.Rpc.Tests.Runtime.Server.ServiceDiscovery
{
    public class DefaultServiceEntryGenerator_Test : LmsIntegratedTest<LmsRpcTestModule>
    {
        private readonly IServiceEntryGenerator _serviceEntryGenerator;

        public DefaultServiceEntryGenerator_Test()
        {
            _serviceEntryGenerator = GetRequiredService<IServiceEntryGenerator>();
        }

        [Fact]
        public void Shuold_CreateServiceEntry()
        {

        }
    }
}