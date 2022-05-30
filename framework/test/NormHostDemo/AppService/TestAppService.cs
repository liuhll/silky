using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Mapster;
using NormHostDemo.AppService.DomainService;
using NormHostDemo.Tests;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Runtime.Session;
using Silky.Core.Serialization;
using Silky.EntityFrameworkCore.Repositories;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Tcc;

namespace NormHostDemo.AppService
{
    [ServiceKey("v1", 3)]
    public class TestAppService : ITestAppService
    {
        private readonly IDistributedCache<TestOut> _distributedCache;
        private readonly IRepository<Test> _testRepository;
        private readonly ISerializer _serializer;
        private readonly ISession _session;
        private readonly IRpcContextAccessor _rpcContextAccessor;

        public TestAppService(
            IDistributedCache<TestOut> distributedCache,
            IRepository<Test> testRepository,
            ISerializer serializer,
            IRpcContextAccessor rpcContextAccessor)
        {
            _distributedCache = distributedCache;
            _testRepository = testRepository;
            _serializer = serializer;
            _rpcContextAccessor = rpcContextAccessor;
            _session = NullSession.Instance;
        }

       // [UnitOfWork]
        public async Task<TestOut> Create(TestInput input)
        {
            var test = input.Adapt<Test>();
            throw new BusinessException("error",1010);
          //  var result = await _testRepository.InsertNowAsync(test);
          // await _anotherAppService.ReturnNullTest();
          // return new TestOut()
          // {
          //     Name = input.Name,
          //     Address = input.Address,
          //     Id = input.Ids.First()
          // };
        }

        public Task CreateOrUpdateAsync(TestInput input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<TestOut> Get(long id)
        {
            var test = await _testRepository.FindAsync(id);
            return test.Adapt<TestOut>();
        }

        public async Task<string> Update(TestInput input)
        {
            throw new BusinessException("test exception");
        }

        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteCancel")]
        public async Task<string> DeleteAsync(TestInput input)
        {
            // await _anotherAppService.DeleteOne(input.Name);
            // await _anotherAppService.DeleteTwo(input.Address);
             throw new BusinessException("test exception");
            return "trying" + _serializer.Serialize(input);
        }

        public async Task<string> DeleteConfirm(TestInput input)
        {
            return "DeleteConfirm" + _serializer.Serialize(input);
        }

        public async Task<string> DeleteCancel(TestInput input)
        {
            return "DeleteCancel" + _serializer.Serialize(input);
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

        public async Task<TestOut> GetById(long? id)
        {
            return new()
            {
                Id = id ?? 0
            };
        }


        public async Task<string> UpdatePart(TestInput input)
        {
            return "UpdatePart";
        }

        public async Task<IList<object>> GetObjectList()
        {
            var objects = new List<object>();
            dynamic obj = new
            {
                Value = "08车10F号",
                Position = new List<int>()
                {
                    730,
                    246,
                    967,
                    249,
                    966,
                    302,
                    729,
                    299
                },
                Key = "seat_number",
                Description = "座位号"
            };
            for (int i = 0; i < 1000; i++)
            {
                objects.Add(obj);
            }

            return objects;
        }

        public async Task<object> GetObject()
        {
            var obj = new
            {
                Value = "08车10F号",
                Position = new List<int>()
                {
                    730,
                    246,
                    967,
                    249,
                    966,
                    302,
                    729,
                    299
                },
                Key = "seat_number",
                Description = "座位号"
            };
            return obj;
        }

        public async Task<OcrOutput> GetOcr()
        {
            var obj = new
            {
                Value = "08车10F号",
                Position = new List<int>()
                {
                    730,
                    246,
                    967,
                    249,
                    966,
                    302,
                    729,
                    299
                },
                Key = "seat_number",
                Description = "座位号"
            };
            var ocrOutput = new OcrOutput()
            {
                Result = obj
            };
            return ocrOutput;
        }

        public async Task<string> TestNamedService(string serviceName)
        {
            var service = EngineContext.Current.ResolveNamed<ITestDomainService>(serviceName);
            if (service == null)
            {
                throw new BusinessException($"不存在{serviceName}的服务");
            }

            return await service.Test();
        }
    }
}