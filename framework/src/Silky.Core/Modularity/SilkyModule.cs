using System;
using Autofac;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Extensions;

namespace Silky.Core.Modularity
{
    public abstract class SilkyModule : Autofac.Module, ISilkyModule, IDisposable
    {
        protected SilkyModule()
        {
            Name = GetType().Name.RemovePostFix(StringComparison.OrdinalIgnoreCase, "Module");
        }

        protected override void Load([NotNull] ContainerBuilder builder)
        {
            base.Load(builder);
            RegisterServices(builder);
        }

        protected virtual void RegisterServices([NotNull] ContainerBuilder builder)
        {
        }

        public virtual void Dispose()
        {
        }

        public static void CheckSilkyModuleType(Type moduleType)
        {
            if (!IsSilkyModule(moduleType))
            {
                throw new ArgumentException("Given type is not an Silky module: " + moduleType.AssemblyQualifiedName);
            }
        }

        public static bool IsSilkyModule(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return
                typeInfo.IsClass &&
                !typeInfo.IsAbstract &&
                !typeInfo.IsGenericType &&
                typeof(ISilkyModule).GetTypeInfo().IsAssignableFrom(type);
        }
        
        public virtual string Name { get; }

        public virtual Task PreInitialize(ApplicationInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task Initialize([NotNull] ApplicationInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task PostInitialize(ApplicationInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task Shutdown([NotNull] ApplicationShutdownContext context)
        {
            return Task.CompletedTask;
        }

        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}