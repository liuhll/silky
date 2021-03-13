using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server;
using Lms.Transaction.Tcc;

namespace NormHostDemo.AppService
{
    [ServiceKey("v1", 3)]
    public class TestAppService : ITestAppService
    {

        private readonly IAnotherAppService _anotherAppService;

        public TestAppService(IAnotherAppService anotherAppService)
        {
            _anotherAppService = anotherAppService;
        }

        public async Task<TestOut> Create(TestInput input)
        {
            return new()
            {
                Address = input.Address,
                Name = input.Name + "v1",
            };
        }

        public async Task<string> Update(TestInput input)
        {
            return "Update";
        }

        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteConcel")]
        public async Task<string> Delete(string name)
        {
            await _anotherAppService.Delete(name);
            return name + " v1";
        }

        public async Task<string> DeleteConfirm(string name)
        {
            return name + " DeleteConfirm v1";
        }

        public async Task<string> DeleteConcel(string name)
        {
            return name + "DeleteConcel v1";
        }

        public Task<string> Search(TestInput query)
        {
            return Task.FromResult("Search");
        }

        public string Form(TestInput query)
        {
            return "Form";
        }

        public async Task<TestOut> Get(string name)
        {
            return new()
            {
                Name = name + "v1"
            };
        }


        public async Task<string> UpdatePart(TestInput input)
        {
            return "UpdatePart";
        }
    }
}