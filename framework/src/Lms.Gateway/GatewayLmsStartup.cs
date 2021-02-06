using Lms.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Gateway
{
    public class GatewayLmsStartup : ILmsStartup
    {

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
           
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order { get; } = 1;
    }
}