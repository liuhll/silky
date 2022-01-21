using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace NormHostDemo.AppService.DomainService;

[InjectNamed("BTest")]
public class BTestDomainService : ITestDomainService
{
    public Task<string> Test()
    {
        return Task.FromResult("BTest");
    }
}