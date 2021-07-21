using Silky.Rpc.Runtime.Server;
using Silky.TestBase.Testing;
using Shouldly;
using Xunit;

namespace Silky.Rpc.Tests.Runtime.Server
{
    public class ServiceEntryManager_Tests : SilkyIntegratedTest<SilkyRpcTestModule>
    {
        private readonly IServiceEntryManager _serviceEntryManager;

        public ServiceEntryManager_Tests()
        {
            _serviceEntryManager = GetRequiredService<IServiceEntryManager>();
        }

        [Fact]
        public void Should_GetEntries()
        {
            _serviceEntryManager.GetAllEntries().ShouldNotBeNull();
        }
    }
}