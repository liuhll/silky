using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NormHostDemo.Contexts;
using Silky.Core;
using Silky.Rpc.SkyApm.Diagnostics;
using SkyApm.Diagnostics.EntityFrameworkCore;

namespace NormHostDemo
{
    public class NormHostConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "NormHostDemo");

            services.AddSkyAPM(extensions => { extensions.AddSilkyRpc().AddEntityFrameworkCore(option =>
            {
                option.AddPomeloMysql();
            }); });
        }

        public int Order { get; } = 10;
    }
}