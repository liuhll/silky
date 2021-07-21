using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Core;
using Silky.Lms.Rpc.SkyApm.Diagnostics;

namespace AnotherHostDemo
{
    public class AnotherHostConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSkyAPM(extensions => { extensions.AddSilkyRpc(); });
        }

        public int Order { get; } = 10;
    }
}