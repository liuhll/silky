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
        //[UnitOfWork]
        // [Fallback(typeof(ICreateFallback))]
        //  [Authorize(Roles = "Administrator, PowerUser")]
        [TestClientFilter(1)]
        [HttpPost]
        [HttpPut]
        [UpdateCachingIntercept("id:{id}")]
        Task<TestOut> CreateOrUpdateAsync(TestInput input);

        // [HttpPost]
        // [HttpPut]
        // Task CreateOrUpdateAsync(TestInput input);

        [AllowAnonymous]
        [HttpGet("{id:long}")]
        [GetCachingIntercept("id:{id}")]
        Task<TestOut> Get( /*[CacheKey(0)]*/ long id);

        [Obsolete]
        [HttpPut("modify")]
        [RemoveCachingIntercept(typeof(TestOut), "id:{Id}")]
        Task<TestOut> Update(TestInput input);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "id:{0}")]
        [Governance(RetryTimes = 2)]
        [HttpDelete]
        Task<string> DeleteAsync([CacheKey(0)] long id);

        [HttpGet]
        Task<PagedList<TestOut>> Search1([FromQuery] string name, [FromQuery] string address,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10);

        [HttpGet]
        Task<PagedList<TestOut>> Search2([FromQuery] SearchInput query);

        [HttpPost]
        [HttpPut]
        Task<TestOut> Form([FromForm] TestInput input);

        [HttpGet("{name}")]
        [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        // [HttpGet("{id:long}")]
        // [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        //  [GetCachingIntercept("id:{0}")]
        //[HttpGet("test1/{id:long}")]
        [HttpGet]
        [AllowAnonymous]
        Task<TestOut> GetById(long? id);

        [HttpPatch]
        [Fallback(typeof(IUpdatePartFallBack))]
        [RemoveCachingIntercept(typeof(TestOut), "id:{Id}")]
        Task<TestOut> UpdatePart(TestUpdatePart input);

        Task<IList<object>> GetObjectList();

        Task<object> GetObject();

        Task<OcrOutput> GetOcr();

        Task<string> TestNamedService(string serviceName);
    }
}