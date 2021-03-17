using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.FallBack;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transport.CachingIntercept;
using Lms.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace ITestApplication.Test
{
    [ServiceRoute(multipleServiceKey: true)]
    public interface ITestAppService
    {
        [GetCachingIntercept("ITestAppService.TestOut", "name:{0}")]
        Task<TestOut> Create(TestInput input);

        Task<string> Update(TestInput input);
        
        [RemoveCachingIntercept("ITestAppService.TestOut", "name:{0}")]
        [Transaction]
        Task<string> Delete([CacheKey(0)]string name);

        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        [HttpPost]
        string Form([FromForm] TestInput query);     

        [HttpGet("{name:string}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("ITestAppService.TestOut", "name:{0}")]
        Task<TestOut> Get([HashKey][CacheKey(0)] string name);

        //[HttpPatch("patch")]
        [HttpPatch]
        [Governance(FallBackType = typeof(UpdatePartFallBack))]
        Task<string> UpdatePart(TestInput input);
    }
}