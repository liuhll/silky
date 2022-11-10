using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class ApplicationContextExtensions
    {
        public static bool IsAddRegistryCenterService(this ApplicationInitializationContext context,
            out string registryCenterType)
        {
            registryCenterType = EngineContext.Current.Configuration.GetValue<string>("RegistryCenter:Type");
            if (registryCenterType.IsNullOrEmpty())
            {
                throw new SilkyException("You must specify a service registry module");
            }

            return context.ServiceProvider.GetService<IServerRegister>() != null;
        }
    }
}