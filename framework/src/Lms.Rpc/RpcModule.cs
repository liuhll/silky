using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Modularity;
using Lms.Rpc.Runtime.Server.ServiceEntry;

namespace Lms.Rpc
{
    public class RpcModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterTypes(
                ServiceEntryHelper.FindServiceEntryTypes(EngineContext.Current.TypeFinder).ToArray())
                .AsSelf()
                .AsImplementedInterfaces();
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var entries = EngineContext.Current.Resolve<IServiceEntryManager>().GetEntries();

        }
    }
}