using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITestApplication.Filters;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.Fallback;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace ITestApplication.Test
{
    [ServiceRoute]
    // [Authorize(Roles = "Administrator, PowerUser")]
    [Authorize]
    public interface ITestAppService
    {
        /// <summary>
        ///  新增接口测试
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [GetCachingIntercept("name:{0}")]
        //[UnitOfWork]
        // [Fallback(typeof(ICreateFallback))]
        //  [Authorize(Roles = "Administrator, PowerUser")]
        [TestClientFilter(1)]
        Task<TestOut> Create(TestInput input);

        // [HttpPost]
        // [HttpPut]
        // Task CreateOrUpdateAsync(TestInput input);

        [AllowAnonymous]
        Task<TestOut> Get(long id);

        [Obsolete]
        [HttpPut]
        Task<string> Update(TestInput input);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
        [Transaction]
        [Governance(RetryTimes = 2)]
        [HttpDelete]
        Task<string> DeleteAsync(TestInput input);

        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        [HttpPost]
        [HttpPut]
        string Form([FromForm] TestInput query);

        [HttpGet("{name}")]
        [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        // [HttpGet("{id:long}")]
        // [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        //  [GetCachingIntercept("id:{0}")]
        // [HttpGet("test/{id:long}")]
        [HttpGet]
        [AllowAnonymous]
        Task<TestOut> GetById(long? id);

        [HttpPatch]
        [Fallback(typeof(IUpdatePartFallBack))]
        Task<string> UpdatePart(TestInput input);

        Task<IList<object>> GetObjectList();

        Task<object> GetObject();

        Task<OcrOutput> GetOcr();

        Task<string> TestNamedService(string serviceName);
    }
}