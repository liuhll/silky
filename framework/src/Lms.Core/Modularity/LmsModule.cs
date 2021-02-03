using System;
using Autofac;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core.Extensions;

namespace Lms.Core.Modularity
{
    public abstract class LmsModule : Autofac.Module, ILmsModule, IDisposable
    {
        protected LmsModule()
        {
            Name = GetType().Name.RemovePostFix(StringComparison.OrdinalIgnoreCase, "Module");
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            RegisterServices(builder);
        }

        protected virtual void RegisterServices(ContainerBuilder builder)
        {
        }

        public virtual void Dispose()
        {
        }

        public static void CheckLmsModuleType(Type moduleType)
        {
            if (!IsLmsModule(moduleType))
            {
                throw new ArgumentException("Given type is not an LMS module: " + moduleType.AssemblyQualifiedName);
            }
        }

        public static bool IsLmsModule(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return
                typeInfo.IsClass &&
                !typeInfo.IsAbstract &&
                !typeInfo.IsGenericType &&
                typeof(ILmsModule).GetTypeInfo().IsAssignableFrom(type);
        }

        public virtual string Name { get; }

        public virtual Task Initialize(ApplicationContext applicationContext)
        {
            return Task.CompletedTask;
        }

        public virtual Task Shutdown(ApplicationContext applicationContext)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}