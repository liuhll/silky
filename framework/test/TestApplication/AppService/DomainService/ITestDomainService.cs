using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace TestApplication.AppService.DomainService;

public interface ITestDomainService : IScopedDependency
{
    Task<string> Test();
}