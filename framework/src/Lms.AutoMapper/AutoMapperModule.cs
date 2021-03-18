using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Modularity;

namespace Lms.AutoMapper
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