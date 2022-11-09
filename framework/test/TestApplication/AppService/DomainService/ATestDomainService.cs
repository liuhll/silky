using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace TestApplication.AppService.DomainService;

[InjectNamed("ATest")]
public class ATestDomainService : ITestDomainService
{
    public Task<string> Test()
    {
        return Task.FromResult("ATest");
    }
}