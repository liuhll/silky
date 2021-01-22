using System;
using Autofac;
using System.Reflection;

namespace Lms.Core.Modularity
{
    public abstract class LmsModule : Autofac.Module, ILmsModule, IDisposable
    {
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

        public virtual void Initialize(ApplicationContext applicationContext)
        {
            
        }

        public virtual void Shutdown(ApplicationContext applicationContext)
        {
            
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}