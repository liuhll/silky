using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.Hosting
{
    public class DefaultWebHostModule : WebHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (!services.IsAdded(typeof(IServerRegister)))
            {
                var registerType = configuration.GetValue<string>("registrycenter:type");
                if (registerType.IsNullOrEmpty())
                {
                    throw new SilkyException("You did not specify the service registry type");
                }

                services.AddDefaultRegistryCenter(registerType);
            }
        }

        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serverRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRegister.RemoveSelf();
        }
    }
}