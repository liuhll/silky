using System.Threading.Tasks;
using Lms.Rpc.Tests.AppService.Dtos;

namespace Lms.Rpc.Tests.AppService
{
    public class TestAppService : ITestAppService
    {
        public Task<long> Create(CreateInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}