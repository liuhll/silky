using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Silky.Core.Configuration;

namespace Silky.TestBase.Testing
{
    public abstract class SilkyIntegratedTest<TStartupModule> : SilkyTestBaseWithServiceProvider, IDisposable
        where TStartupModule : SilkyModule
    {
        protected IServiceScope TestServiceScope { get; }

        protected IConfiguration Configuration { get; }

        protected IEngine Engine { get; }


        protected SilkyIntegratedTest()
        {
            var services = CreateServiceCollection();
            BeforeAddApplication(services);

            Configuration = CreateConfigurationBuilder().Build();
            var hostEnvironment = CreateHostEnvironment();
            services.AddSingleton(Configuration);

            Engine = services.AddSilkyServices<TStartupModule>(Configuration, hostEnvironment,
                new SilkyApplicationCreationOptions(services));

            AfterAddApplication(services);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(services);

            Engine.RegisterDependencies(containerBuilder);
            Engine.RegisterModules(services, containerBuilder);

            var container = containerBuilder.Build();
            ServiceProvider = new AutofacServiceProvider(container);
            Engine.ServiceProvider = ServiceProvider;
            TestServiceScope = ServiceProvider.CreateScope();
        }

        protected virtual IHostEnvironment CreateHostEnvironment()
        {
            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.EnvironmentName)
                .Returns("Testing");
            mockEnvironment.Setup(e => e.ApplicationName)
                .Returns(this.GetType().Name);
            mockEnvironment.Setup(m => m.ContentRootPath)
                .Returns(Directory.GetCurrentDirectory());
            return mockEnvironment.Object;
        }

        protected virtual IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        protected virtual IConfigurationBuilder CreateConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
        }

        protected virtual void BeforeAddApplication(IServiceCollection services)
        {
        }


        protected virtual void AfterAddApplication(IServiceCollection services)
        {
        }


        public void Dispose()
        {
            TestServiceScope.Dispose();
        }
    }
}