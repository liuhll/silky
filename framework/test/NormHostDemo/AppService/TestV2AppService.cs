using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server;

namespace NormHostDemo.AppService
{
    [ServiceKey("v2",2)]
    public class TestV2AppService : ITestAppService
    {
        public async Task<string> Create(TestDto input)
        {
            return "Create.v2";
        }

        public Task<string> Update(TestDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Search(TestDto query)
        {
            throw new System.NotImplementedException();
        }

        public string Form(TestDto query)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Get(long id, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> UpdatePart(TestDto input)
        {
            throw new System.NotImplementedException();
        }
    }
}