using System;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.FallBack;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Parameter;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.CachingInterceptor;

namespace ITestApplication.Test
{
    [ServiceRoute(multipleServiceKey: true)]
    public interface ITestAppService
    {
        /// <summary>
        ///  新增接口测试
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //[GetCachingIntercept("name:{0}")]
        //[UnitOfWork]
        Task<TestOut> Create(TestInput input);

        Task<TestOut> Get(long id);

        [Obsolete]
        Task<string> Update(TestInput input);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
        [Transaction]
        Task<string> Delete(TestInput input);

        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        [HttpPost]
        [HttpPut]
        string Form([FromForm] TestInput query);

        [HttpGet("{name:string}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        [HttpGet("{id:long}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("id:{0}")]
        Task<TestOut> GetById([HashKey] [CacheKey(0)] long id);

        //[HttpPatch("patch")]
        [HttpPatch]
        [Governance(FallBackType = typeof(UpdatePartFallBack))]
        Task<string> UpdatePart(TestInput input);
    }
}