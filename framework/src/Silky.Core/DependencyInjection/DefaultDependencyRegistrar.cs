using System.Reflection;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Silky.Core.DependencyInjection
{
    public class DefaultDependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            var refAssemblies = typeFinder.GetAssemblies();
            foreach (var assembly in refAssemblies)
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(ISingletonDependency).GetTypeInfo().IsAssignableFrom(t))
                    .PropertiesAutowired()
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance()
                    .PropertiesAutowired()
                    ;

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(ITransientDependency).GetTypeInfo().IsAssignableFrom(t))
                    .PropertiesAutowired()
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .InstancePerDependency();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(IScopedDependency).GetTypeInfo().IsAssignableFrom(t))
                    .PropertiesAutowired()
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }

        public int Order => 1;
    }
}