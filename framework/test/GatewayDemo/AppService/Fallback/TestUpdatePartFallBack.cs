using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.Fallback;
using Silky.Core.DependencyInjection;

namespace GatewayDemo.AppService.Fallback
{
    public class TestUpdatePartFallBack : IUpdatePartFallBack, IScopedDependency
    {
        public async Task<string> UpdatePart(TestInput input)
        {
            return "this is a fallback method for update part";
        }
    }
}