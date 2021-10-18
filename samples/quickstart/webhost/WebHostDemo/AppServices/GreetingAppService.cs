using System.Threading.Tasks;

namespace WebHostDemo.AppServices
{
    public class GreetingAppService : IGreetingAppService
    {
        public Task<string> Get()
        {
            return Task.FromResult("Hello World");
        }
    }
}