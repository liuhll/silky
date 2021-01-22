using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Lms.Core
{
    public class SetServiceProviderHostedService : IHostedService
    {
        public SetServiceProviderHostedService(IServiceProvider serviceProvider)
        {
            if (EngineContext.Current is LmsEngine && ((LmsEngine)EngineContext.Current).ServiceProvider == null)
            {
                ((LmsEngine) EngineContext.Current).ServiceProvider = serviceProvider;
            }
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}