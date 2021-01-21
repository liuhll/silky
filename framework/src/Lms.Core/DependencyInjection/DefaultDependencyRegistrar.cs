using System.Reflection;
using Autofac;
using Lms.Core.Configuration;

namespace Lms.Core.DependencyInjection
{
    public class DefaultDependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            var refAssemblies = typeFinder.GetAssemblies();
            foreach (var assembly in refAssemblies)
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(ISingletonDependency).GetTypeInfo().IsAssignableFrom(t))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance();
                
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(ITransientDependency).GetTypeInfo().IsAssignableFrom(t))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .InstancePerDependency();
                
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(IScopedDependency).GetTypeInfo().IsAssignableFrom(t))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }

        public int Order => 1;
    }
}