using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.FallBack;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace ITestApplication.Test
{
    [ServiceRoute]
    public interface ITestAppService
    {
        Task<string> Create(TestDto input);

        Task<string> Update(TestDto input);

        [HttpGet]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task<string> Search([FromQuery] TestDto query);

        [HttpPost]
        string Form([FromForm] TestDto query);

        [HttpGet("{id:long}")]
        Task<string> Get(long id, string name);

        //[HttpPatch("patch")]
        [HttpPatch]
        [Governance(FallBackType = typeof(UpdatePartFallBack))]
        Task<string> UpdatePart(TestDto input);
    }
}