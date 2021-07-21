using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using TanvirArjel.EFCore.GenericRepository;

namespace Silky.Order.EntityFrameworkCore
{
    public class EfCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderDbContext>(opt =>
                    opt.UseMySql(configuration.GetConnectionString("Default"),
                        ServerVersion.AutoDetect(configuration.GetConnectionString("Default"))))
                .AddGenericRepository<OrderDbContext>(ServiceLifetime.Transient)
                ;
        }

        public int Order { get; } = 1;
    }
}