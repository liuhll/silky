using Silky.Lms.Rpc.Runtime;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Tests.AppService;
using Silky.Lms.TestBase.Testing;
using Xunit;

namespace Silky.Lms.Rpc.Tests.Runtime
{
    public class DefaultServiceIdGenerator_Test : LmsIntegratedTest<LmsRpcTestModule>
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;

        public DefaultServiceIdGenerator_Test()
        {
            _serviceIdGenerator = GetRequiredService<IServiceIdGenerator>();
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