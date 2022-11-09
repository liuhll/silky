using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Silky.Core.Reflection;

namespace Silky.Core.DependencyInjection;

public class NamedServiceDependencyRegistrar : IDependencyRegistrar
{
    public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
    {
        var namedServices = typeFinder.GetAssemblies().SelectMany(a =>
            a.GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && p.GetCustomAttributes().OfType<InjectNamedAttribute>().Any()));
        foreach (var namedService in namedServices)
        {
            var typeInterfaces = namedService.GetInterfaces();
            if (typeInterfaces.Any(t => typeof(ISingletonDependency).IsAssignableFrom(t)))
            {
                foreach (var typeInterface in typeInterfaces)
                {
                    ConfigureBasePropertiesMethodInfo
                        .MakeGenericMethod(typeInterface)
                        .Invoke(this, new object[] { builder, namedService, DependencyType.Singleton });
                }
            }
            else if (typeInterfaces.Any(t => typeof(ITransientDependency).IsAssignableFrom(t)))
            {
                foreach (var typeInterface in typeInterfaces)
                {
                    ConfigureBasePropertiesMethodInfo
                        .MakeGenericMethod(typeInterface)
                        .Invoke(this, new object[] { builder, namedService, DependencyType.Transient });
                }
            }
            else  if (typeInterfaces.Any(t => typeof(IScopedDependency).IsAssignableFrom(t)))
            {
                foreach (var typeInterface in typeInterfaces)
                {
                    ConfigureBasePropertiesMethodInfo
                        .MakeGenericMethod(typeInterface)
                        .Invoke(this, new object[] { builder, namedService, DependencyType.Scoped });
                }
            }
            else
            {
                foreach (var typeInterface in typeInterfaces)
                {
                    ConfigureBasePropertiesMethodInfo
                        .MakeGenericMethod(typeInterface)
                        .Invoke(this, new object[] { builder, namedService, DependencyType.Transient });
                }
            }
        }
    }

    private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
        = typeof(NamedServiceDependencyRegistrar)
            .GetMethod(
                nameof(RegisterNamedService),
                BindingFlags.Instance | BindingFlags.NonPublic
            );

    private void RegisterNamedService<T>(ContainerBuilder builder, Type serviceType, DependencyType dependencyType)
    {
        var registrationBuilder = builder.RegisterType(serviceType)
            .Named<T>(serviceType.GetCustomAttributes().OfType<InjectNamedAttribute>().First().Name);
        switch (dependencyType)
        {
            case DependencyType.Singleton:
                registrationBuilder.SingleInstance();
                break;
            case DependencyType.Transient:
                registrationBuilder.InstancePerDependency();
                break;
            case DependencyType.Scoped:
                registrationBuilder.InstancePerLifetimeScope();
                break;
        }
    }

    public int Order { get; } = 2;

    private enum DependencyType
    {
        Singleton,
        Transient,
        Scoped
    }
}