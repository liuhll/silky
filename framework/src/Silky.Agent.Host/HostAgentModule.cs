using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Modularity;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.Hosting;

public abstract class HostAgentModule : SilkyModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        if (!services.IsAdded(typeof(IServerRegister)))
        {
            var registerType = configuration.GetValue<string>("registrycenter:type");
            if (registerType.IsNullOrEmpty())
            {
                throw new SilkyException("You did not specify the service registry type");
            }

            services.AddDefaultRegistryCenter(registerType);
        }
    }
    
    public override async Task Initialize(ApplicationInitializationContext context)
    {
        var options = EngineContext.Current.GetOptions<RpcOptions>();
        var logger = EngineContext.Current.Resolve<ILogger<HostAgentModule>>();
        var serverRouteRegister =
            context.ServiceProvider.GetRequiredService<IServerRegister>();
        var policy = Policy
            .Handle<TimeoutException>()
            .WaitAndRetryAsync(options.RegisterFailureRetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                async (exception, timeSpan, context) =>
                {
                    if (exception != null)
                    {
                        logger.LogException(exception);
                    }

                    await serverRouteRegister.RegisterServer();
                });
        await policy.ExecuteAsync(async () => { await serverRouteRegister.RegisterServer(); });
    }

    public override async Task Shutdown(ApplicationShutdownContext context)
    {
        var serverRegister =
            context.ServiceProvider.GetRequiredService<IServerRegister>();
        await serverRegister.RemoveSelf();
    }
}