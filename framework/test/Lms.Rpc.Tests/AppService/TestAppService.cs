using System.Threading.Tasks;
using Silky.Lms.Rpc.Tests.AppService.Dtos;

namespace Silky.Lms.Rpc.Tests.AppService
{
    public class TestAppService : ITestAppService
    {
        public Task<long> Create(TestInput input, string test)
        {
            throw new System.NotImplementedException();
        }

        public Task<long> Update(TestInput input)
        {
            throw new System.NotImplementedException();
        }

        public Task<long> Query(TestInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}