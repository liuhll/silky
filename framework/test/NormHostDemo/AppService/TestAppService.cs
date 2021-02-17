using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server;

namespace NormHostDemo.AppService
{
    [ServiceKey("v1", 3)]
    public class TestAppService : ITestAppService
    {
        public async Task<string> Create(TestDto input)
        {
            return "Create";
        }

        public async Task<string> Update(TestDto input)
        {
            return "Update";
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

        public async Task<string> UpdatePart(TestDto input)
        {
            return "UpdatePart";
        }
    }
}