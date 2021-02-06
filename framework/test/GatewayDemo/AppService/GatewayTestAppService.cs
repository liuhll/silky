using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace GatewayDemo.AppService
{
    public class GatewayTestAppService : ITestAppService
    {
        public async Task<TestOut> Create(TestDto input)
        {
            return new TestOut() { Address = "test", Name = "test"};
        }

        public Task<string> Update(TestDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<TestOut> Search(TestDto query)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Form(TestDto query)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Get(long id, string name)
        {
            throw new System.NotImplementedException();
        }
    }
}