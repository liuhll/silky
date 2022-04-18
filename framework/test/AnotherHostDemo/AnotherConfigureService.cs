using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AnotherHostDemo
{
    public class AnotherConfigureService : IConfigureService
    {
        public ILogger<AnotherConfigureService> Logger { get; set; }

        public AnotherConfigureService()
        {
            Logger = NullLogger<AnotherConfigureService>.Instance;
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
            // services.AddMessagePackCodec();
        }
    }
}