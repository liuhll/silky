using Autofac;
using Lms.Core.Modularity;
using Lms.Rpc;

namespace ConsoleDemo
{
    [DependsOn(typeof(RpcModule))]
    public class ConsoleDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}