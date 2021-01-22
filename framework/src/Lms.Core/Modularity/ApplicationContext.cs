using System;

namespace Lms.Core.Modularity
{
    public class ApplicationContext
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public ApplicationContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}