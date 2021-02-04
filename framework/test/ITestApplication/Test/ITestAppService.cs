using System.Threading.Tasks;
using ConsoleDemo.AppService.Dtos;
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
        Task<string> Search(TestDto query);
        
        [HttpGet("{id:long}")]
        Task<string> Get(long id);
    }
}