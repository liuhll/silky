using System.Threading.Tasks;
using Application.Contracts.AppServices;

namespace GeneralHostDemo.AppServices
{
    public class DemoAppService : IDemoAppService
    {
        public Task<string> Say(string line)
        {
            return Task.FromResult($"Hello {line}");
        }
    }
}