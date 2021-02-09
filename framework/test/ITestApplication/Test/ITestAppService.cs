using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
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
        Task<string> Search([FromQuery]TestDto query);
        
        [HttpPost]
        Task<string> Form([FromForm]TestDto query);
        
        [HttpGet("{id:long}")]
        Task<string> Get(long id,string name);
        
        //[HttpPatch("patch")]
        [HttpPatch]
        Task<string> UpdatePart(TestDto input);
    }
}