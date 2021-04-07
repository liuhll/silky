using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.AutoMapper
{
    public class AutoMapperModule : LmsModule
    {
        public override Task Initialize(ApplicationContext applicationContext)
        {
            var autoMapperBootstrap = EngineContext.Current.Resolve<IAutoMapperBootstrap>();
            return Task.Factory.StartNew(()=> autoMapperBootstrap.Initialize());
        }
    }
}