using Autofac;
using Silky.Core.Reflection;

namespace Silky.Core.DependencyInjection
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);

        int Order { get; }
    }
}