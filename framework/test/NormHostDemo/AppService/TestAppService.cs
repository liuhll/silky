using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Silky.Lms.Caching;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Transaction.Tcc;

namespace NormHostDemo.AppService
{
    [ServiceKey("v1", 3)]
    public class TestAppService : ITestAppService
    {

        private readonly IAnotherAppService _anotherAppService;
        private readonly IDistributedCache<TestOut> _distributedCache;
        public TestAppService(IAnotherAppService anotherAppService, 
            IDistributedCache<TestOut> distributedCache)
        {
            _anotherAppService = anotherAppService;
            _distributedCache = distributedCache;
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
            throw new BusinessException("test exception");
        }

        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteCancel")]
        public async Task<string> Delete(string name)
        {
            await _anotherAppService.DeleteOne(name);
            await _anotherAppService.DeleteTwo(name);
            return name + " v1";
        }

        public async Task<string> DeleteConfirm(string name)
        {
            return name + " DeleteConfirm v1";
        }

        public async Task<string> DeleteCancel(string name)
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

        public async Task<TestOut> GetById(long id)
        {
            return new()
            {
                Id = id
            };
        }


        public async Task<string> UpdatePart(TestInput input)
        {
            return "UpdatePart";
        }
    }
}