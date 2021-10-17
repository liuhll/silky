using System.Threading.Tasks;

namespace WebHostDemo.AppServices
{
    public class HelloWorldAppService : IHelloWorldAppService
    {
        public Task<string> Get()
        {
            return Task.FromResult("Hello World");
        }
    }
}