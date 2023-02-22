using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Extensions;
using WsHostDemo.Mq.Consumer;

namespace WsHostDemo
{
    public class WsConfigureService : IConfigureService
    {
        public ILogger<WsConfigureService> Logger { get; set; }

        public WsConfigureService()
        {
            Logger = NullLogger<WsConfigureService>.Instance;
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