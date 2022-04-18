using System.Threading.Tasks;
using Silky.Rpc.Routing;

namespace ITestApplication.Test;

[ServiceRoute]
public interface ITemplateTestAppService
{
    Task<string> CallCreateOne(string name);
    
    Task<dynamic> CallTest();
}