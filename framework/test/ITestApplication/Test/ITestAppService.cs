using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace ITestApplication.Test
{
    [ServiceBundle]
    public interface ITestAppService
    {
        Task<string> Create(TestDto input);

        Task<string> Update(TestDto input);

        [HttpGet]
        Task<string> Search([FromQuery]TestDto query);
        
        [HttpPost]
        Task<string> Form([FromForm]TestDto query);
        
        [HttpGet("{id:long}")]
        Task<string> Get(long id,[Required(ErrorMessage = "姓名不允许为空")]string name);
    }
}