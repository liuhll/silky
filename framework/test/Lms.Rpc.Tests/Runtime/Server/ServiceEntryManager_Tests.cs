using Lms.Rpc.Runtime.Server;
using Lms.TestBase.Testing;
using Shouldly;
using Xunit;

namespace Lms.Rpc.Tests.Runtime.Server
{
    public class ServiceEntryManager_Tests : LmsIntegratedTest<LmsRpcTestModule>
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