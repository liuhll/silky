
using Silky.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.WebHost
{
    public class WebHostStartUp : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
        }
        public int Order { get; } = 1;
    }
}
