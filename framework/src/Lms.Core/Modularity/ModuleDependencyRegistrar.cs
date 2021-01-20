using Autofac;
using Lms.Core.Configuration;
using Lms.Core.DependencyInjection;

namespace Lms.Core.Modularity
{
    public class ModuleDependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
           
        }

        public int Order => 2;
    }
}