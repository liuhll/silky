using Autofac;
using Lms.Core.Configuration;

namespace Lms.Core.DependencyInjection
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings);
        
        int Order { get; }
    }
}