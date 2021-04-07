using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Core;
using TanvirArjel.EFCore.GenericRepository;

namespace Lms.Account.EntityFrameworkCore
{
    public class EfCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(opt =>
                    opt.UseMySql(configuration.GetConnectionString("Default"),
                        ServerVersion.AutoDetect(configuration.GetConnectionString("Default"))))
                .AddGenericRepository<UserDbContext>(ServiceLifetime.Transient)
                ;
        }

        public int Order { get; } = 1;
    }
}