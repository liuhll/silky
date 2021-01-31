using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Modularity;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Lms.Rpc.Utils;
using Microsoft.Extensions.Options;

namespace Lms.Rpc
{
    public class RpcModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterTypes(
                ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder).ToArray())
                .AsSelf()
                .AsImplementedInterfaces();
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var host = NetUtil.GetHostAddress();
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var entries = EngineContext.Current.Resolve<IServiceEntryManager>().GetAllEntries();
            
        }
    }
}