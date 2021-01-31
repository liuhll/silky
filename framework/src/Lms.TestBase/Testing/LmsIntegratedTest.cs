using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core;
using Lms.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Lms.TestBase.Testing
{
    public abstract class LmsIntegratedTest<TStartupModule> : LmsTestBaseWithServiceProvider, IDisposable
        where TStartupModule : ILmsModule
    {

        protected IServiceScope TestServiceScope { get; }

        protected IConfiguration Configuration { get; }

        protected IEngine Engine { get; }


        protected LmsIntegratedTest()
        {
            var services = CreateServiceCollection();
            BeforeAddApplication(services);
            
            Configuration = CreateConfigurationBuilder().Build();
            var hostEnvironment = CreateHostEnvironment();
            services.AddSingleton(Configuration);
            
            Engine = services.ConfigureLmsServices(Configuration,hostEnvironment);

            AfterAddApplication(services);

            var containerBuilder = new ContainerBuilder();
            
            containerBuilder.Populate(services);
            
            Engine.RegisterDependencies(containerBuilder);
            Engine.RegisterModules<TStartupModule>(services,containerBuilder);

            var container = containerBuilder.Build(); 
            ServiceProvider = new AutofacServiceProvider(container);
            ((LmsEngine)Engine).ServiceProvider = ServiceProvider;
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