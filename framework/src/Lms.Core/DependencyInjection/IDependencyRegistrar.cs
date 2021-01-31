using Autofac;

namespace Lms.Core.DependencyInjection
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);
        
        int Order { get; }
    }
}