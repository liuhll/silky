using System.Threading.Tasks;
using ConsoleDemo.AppService.Dtos;
using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleDemo.AppService
{
    [ServiceBundle("{AppService}/{Method}")]
    public interface ITestAppService
    {
        Task<string> Create(TestDto input);

        Task<string> Update(TestDto input);

        [HttpGet]
        Task<string> Search(TestDto query);
    }
}