using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Extensions;
using TestApplication.Contexts;

namespace NormHostDemo
{
    public class NormHostConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
            services.AddJwt();
            services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "TestApplication");
        
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(configuration["rabbitMq:host"], 
                        configuration["rabbitMq:port"].To<ushort>(),
                        configuration["rabbitMq:virtualHost"], 
                        config =>
                        {
                            config.Username(configuration["rabbitMq:userName"]);
                            config.Password(configuration["rabbitMq:password"]);
                        });
                });
            });
        }
    }
}