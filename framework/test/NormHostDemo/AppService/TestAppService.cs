using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Mapster;
using NormHostDemo.Tests;
using Silky.Caching;
using Silky.Core.Exceptions;
using Silky.EntityFrameworkCore.Repositories;
using Silky.EntityFrameworkCore.UnitOfWork;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Tcc;

namespace NormHostDemo.AppService
{
    [ServiceKey("v1", 3)]
    public class TestAppService : ITestAppService
    {

        private readonly IAnotherAppService _anotherAppService;
        private readonly IDistributedCache<TestOut> _distributedCache;
        private readonly IRepository<Test> _testRepository;
        public TestAppService(IAnotherAppService anotherAppService, 
            IDistributedCache<TestOut> distributedCache,
            IRepository<Test> testRepository)
        {
            _anotherAppService = anotherAppService;
            _distributedCache = distributedCache;
            _testRepository = testRepository;
        }
        
        public async Task<TestOut> Create(TestInput input)
        {
            var test = input.Adapt<Test>();
            var result = await _testRepository.InsertNowAsync(test);
           // throw new BusinessException("error");
           return new TestOut()
            {
                Id = result.Entity.Id,
                Name = result.Entity.Name,
                Address = result.Entity.Address
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
          //  throw new BusinessException("test exception");
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