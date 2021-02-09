using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace NormHostDemo.AppService
{
    public class TestAppService : ITestAppService
    {
        
        public async Task<string> Create(TestDto input)
        {
            return "Create";
        }

        public void Update(TestDto input)
        {
            
        }

        public Task<string> Search(TestDto query)
        {
            return Task.FromResult("Search");
        }

        public string Form(TestDto query)
        {
            return "Form";
        }

        public async Task<string> Get(long id, string name)
        {
            return "Get";
        }

        public async Task UpdatePart(TestDto input)
        {
            
        }
    }
}