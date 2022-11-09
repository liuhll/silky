using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace TestApplication.AppService.DomainService;

[InjectNamed("BTest")]
public class BTestDomainService : ITestDomainService
{
    public Task<string> Test()
    {
        return Task.FromResult("BTest");
    }
}