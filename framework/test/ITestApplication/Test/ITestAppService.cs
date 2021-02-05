using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace ITestApplication.Test
{
    [ServiceRoute]
    public interface ITestAppService
    {
        Task<TestOut> Create(TestDto input);

        Task<string> Update(TestDto input);

        [HttpGet]
        Task<TestOut> Search([FromQuery]TestDto query);
        
        [HttpPost]
        Task<string> Form([FromForm]TestDto query);
        
        [HttpGet("{id:long}")]
        Task<string> Get(long id,[Required(ErrorMessage = "姓名不允许为空")]string name);
    }
}