using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace NormHostDemo.AppService.DomainService;

[InjectNamed("ATest")]
public class ATestDomainService : ITestDomainService
{
    public Task<string> Test()
    {
        return Task.FromResult("ATest");
    }
}