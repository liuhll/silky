using System.Threading.Tasks;
using IAnotherApplication.Dtos;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Auditing;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace IAnotherApplication
{
    [ServiceRoute(ServiceName = "AnotherAppService")]
    [DisableAuditing]
    [Authorize]
    public interface IAnotherAppService
    {
        [Transaction]
        [HttpDelete("one")]
        [Governance(ProhibitExtranet = true)]
        Task<string> DeleteOne(string name);

        [Transaction]
        [HttpDelete("two")]
        [Governance(ProhibitExtranet = true)]
        Task<string> DeleteTwo(string address);

        Task<string> Create(string name);

        [GetCachingIntercept("another:name:{name}")]
        Task<TestDto> Test(TestDto input);

        [HttpGet("query1/{id:long}")]
        [GetCachingIntercept("Query1:id{id}:code:{code}")]
        Task<string> Query1(long id, long? code);

        [HttpPut("update/{id:long}")]
        [UpdateCachingIntercept("Query1:id{id}:code:{code}")]
        Task<string> Update(long id, long? code);

        [HttpDelete("delete/{id:long}")]
        [RemoveMatchKeyCachingIntercept(typeof(string), "Query1:id{id}:code:*")]
        Task<string> Delete(long id);

        Task ReturnNullTest();
    }
}