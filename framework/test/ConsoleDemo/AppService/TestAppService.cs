using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace ConsoleDemo.AppService
{
    public class TestAppService : ITestAppService
    {
        public Task<string> CreateTest(TestDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Create(TestDto input)
        {
            throw new System.NotImplementedException();
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