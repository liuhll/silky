using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;
using Lms.Rpc.Tests.AppService.Dtos;

namespace Lms.Rpc.Tests.AppService
{
    [ServiceBundle("api/test")]
    public interface ITestAppService
    {
        Task<long> Create(CreateInput input);
    }
}