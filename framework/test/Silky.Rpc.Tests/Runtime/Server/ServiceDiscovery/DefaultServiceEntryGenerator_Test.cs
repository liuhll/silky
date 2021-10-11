using Silky.Rpc.Runtime.Server;
using Silky.TestBase.Testing;
using Xunit;

namespace Silky.Rpc.Tests.Runtime.Server.ServiceDiscovery
{
    public class DefaultServiceEntryGenerator_Test : SilkyIntegratedTest<SilkyRpcTestModule>
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