using System;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.TestBase
{
    public abstract class SilkyTestBaseWithServiceProvider
    {
        protected IServiceProvider ServiceProvider { get; set; }

        protected virtual T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        protected virtual T GetRequiredService<T>()
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}