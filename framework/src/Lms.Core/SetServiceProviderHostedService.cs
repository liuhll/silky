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
            if (EnginContext.Current is LmsEngine && ((LmsEngine)EnginContext.Current).ServiceProvider == null)
            {
                ((LmsEngine) EnginContext.Current).ServiceProvider = serviceProvider;
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