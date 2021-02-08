using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace NormHostDemo.AppService
{
    public class TestAppService : ITestAppService
    {
        
        public async Task<string> Create(TestDto input)
        {
            return "OK";
        }

        public Task<string> Update(TestDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Search(TestDto query)
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