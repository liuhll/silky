using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.Fallback;
using ITestApplication.Test.Filters;
using Silky.Rpc.Runtime.Server;
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
        [Fallback(typeof(ICreateFallback))]
        //  [Authorize(Roles = "Administrator, PowerUser")]
        [HttpPost]
        [HttpPut]
        [UpdateCachingIntercept("id:{id}")]
   
        Task<TestOut> CreateOrUpdateAsync(TestInput input);
        
        
        [HttpPost("v2")]
        [HttpPut("v2")]
        [TestServerFilter]
        Task CreateOrUpdate2Async(TestInput input);

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
        
        
        [HttpPut("modify2")]
        Task<string> Update2(long id);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "id:{id}")]
        [Governance(RetryTimes = 2)]
        [HttpDelete]
        Task<string> DeleteAsync(long id);

        [HttpGet]
        Task<IDictionary<string, string>> TestDict1();
        

        [HttpGet]
        Task<PagedList<TestOut>> Search1([FromQuery] string name, [FromQuery] string address,
            [FromQuery] IList<long> ids,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10);

        [HttpGet]
        Task<PagedList<TestOut>> Search2([FromQuery] SearchInput query);
        
        Task<PagedList<TestOut>> Search3([FromQuery]long[] Ids,[FromQuery] Sort[] sorts);

        [HttpPost]
        [HttpPut]
        Task<TestOut> Form([FromForm] TestInput input);

        [HttpGet("{name}")]
        [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        [GetCachingIntercept("name:{name}")]
        Task<TestOut> Get1(string name);

        // [HttpGet("{id:long}")]
        // [Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
        //  [GetCachingIntercept("id:{0}")],OrderDetailDto[] orderDetails
        //[HttpGet("test1/{id:long}")]
        [HttpGet]
        [AllowAnonymous]
        Task<TestOut> GetById(long? id);

        [HttpPatch]
        [Fallback(typeof(IUpdatePartFallBack))]
        [RemoveCachingIntercept(typeof(TestOut), "id:{Id}")]
        Task<TestOut> UpdatePart(TestUpdatePart input);

        [HttpGet]
        Task<TestOut> TestFromHeader([FromHeader]string id);
        
        [HttpGet]
        [GetCachingIntercept("get:{name}")]
        Task<TestOut> TestCache(TestInput input);

        Task<IList<object>> GetObjectList();
        
        Task<string> InvokeQuery1();

        Task<object> GetObject();

        Task<OcrOutput> GetOcr();

        Task<string> TestNamedService(string serviceName);
    }
}