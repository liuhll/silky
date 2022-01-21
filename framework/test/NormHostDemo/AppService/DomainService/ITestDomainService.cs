using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace NormHostDemo.AppService.DomainService;

public interface ITestDomainService : IScopedDependency
{
    Task<string> Test();
}