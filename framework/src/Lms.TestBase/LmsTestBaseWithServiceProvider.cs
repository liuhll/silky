using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.TestBase
{
    public abstract class LmsTestBaseWithServiceProvider
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