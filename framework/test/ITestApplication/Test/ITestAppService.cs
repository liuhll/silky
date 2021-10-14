using System;
using System.Threading.Tasks;
using ITestApplication.Filters;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.Fallback;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace ITestApplication.Test
{
    [ServiceRoute]
    // [Authorize(Roles = "Administrator, PowerUser")]
    // [Authorize]
    public interface ITestAppService
    {
        /// <summary>
        ///  新增接口测试
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //[GetCachingIntercept("name:{0}")]
        //[UnitOfWork]
        [Fallback(typeof(ICreateFallback))]
        [Authorize(Roles = "Administrator, PowerUser")]
        [TestClientFilter(1)]
        Task<TestOut> Create(TestInput input);

        [AllowAnonymous]
        Task<TestOut> Get(long id);

        [Obsolete]
        Task<string> Update(TestInput input);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
        [Transaction]
        // [Governance(TimeoutMillSeconds = 5, RetryTimes = 2)]
        Task<string> Delete(TestInput input);

        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        [HttpPost]
        [HttpPut]
        string Form([FromForm] TestInput query);

        [HttpGet("{name}")]
        [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        [HttpGet("{id:long}")]
        [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        [GetCachingIntercept("id:{0}")]
        Task<TestOut> GetById([HashKey] [CacheKey(0)] long id);

        [HttpPatch]
        [Fallback(typeof(IUpdatePartFallBack))]
        Task<string> UpdatePart(TestInput input);
    }
}