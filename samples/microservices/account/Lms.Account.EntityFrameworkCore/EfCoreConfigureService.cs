using Arch.EntityFrameworkCore.UnitOfWork;
using Lms.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Account.EntityFrameworkCore
{
    public class EfCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(opt =>
                    opt.UseMySql(configuration.GetConnectionString("Default")))
                .AddUnitOfWork<UserDbContext>()
                ;
        }

        public int Order { get; } = 1;
    }
}