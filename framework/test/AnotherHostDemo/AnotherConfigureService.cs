using AnotherHostDemo.Mq.Consumer;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Extensions;

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
                    configurator.ReceiveEndpoint("events-listener",
                        e => { e.Consumer<TestConsumer>(); });
                });
                services.AddMassTransitHostedService();
            });
        }
    }
}