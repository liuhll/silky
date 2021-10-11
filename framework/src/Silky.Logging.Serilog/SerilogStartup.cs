using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Silky.Logging.Serilog
{
    public class SerilogStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public int Order { get; } = -1;

        public void Configure(IApplicationBuilder application)
        {
            if (application.ApplicationServices.GetService<DiagnosticContext>() != null)
            {
                application.UseSerilogRequestLogging();
            }
        }
    }
}