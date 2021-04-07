using Autofac;

namespace Silky.Lms.Core.DependencyInjection
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);
        
        int Order { get; }
    }
}