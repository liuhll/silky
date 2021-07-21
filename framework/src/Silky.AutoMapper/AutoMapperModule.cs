using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Modularity;

namespace Silky.AutoMapper
{
    public class AutoMapperModule : SilkyModule
    {
        public override Task Initialize(ApplicationContext applicationContext)
        {
            var autoMapperBootstrap = EngineContext.Current.Resolve<IAutoMapperBootstrap>();
            return Task.Factory.StartNew(()=> autoMapperBootstrap.Initialize());
        }
    }
}