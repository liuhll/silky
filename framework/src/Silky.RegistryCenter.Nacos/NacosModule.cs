using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Medallion.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc;

namespace Silky.RegistryCenter.Nacos
{
    [DependsOn(typeof(RpcModule))]
    public class NacosModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNacosRegistryCenter(NacosRegistryCenterOptions.RegistryCenterSection);
        }

        public override Task PreInitialize(ApplicationInitializationContext context)
        {

            if (!context.ServiceProvider.GetAutofacRoot().IsRegistered(typeof(IDistributedLockProvider)))
            {
                throw new SilkyException(
                    "You must specify the implementation of IDistributedLockProvider in the Silky.RegistryCenter.Nacos project of the distributed transaction");
            }

            return Task.CompletedTask;
        }
    }
}