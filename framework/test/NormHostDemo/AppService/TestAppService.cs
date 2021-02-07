using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace NormHostDemo.AppService
{
    public class TestAppService : ITestAppService
    {
        public Task<string> CreateTest(TestDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<TestOut> Create(TestDto input)
        {
            throw new System.NotImplementedException();
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