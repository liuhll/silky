using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Tests.AppService;
using Silky.TestBase.Testing;
using Xunit;

namespace Silky.Rpc.Tests.Runtime
{
    public class DefaultServiceIdGenerator_Test : SilkyIntegratedTest<SilkyRpcTestModule>
    {
        private readonly IIdGenerator _idGenerator;

        public DefaultServiceIdGenerator_Test()
        {
            _idGenerator = GetRequiredService<IIdGenerator>();
        }

        [Fact]
        public void Should_GenerateServiceId()
        {
            var type = typeof(ITestAppService);
            // foreach (var method in type.GetMethods())
            // {
            //     _serviceIdGenerator.GenerateServiceId(method).ShouldNotBeEmpty();
            // }
        }
    }
}