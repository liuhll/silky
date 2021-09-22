using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;

namespace Silky.Rpc.Extensions
{
    public static class ApplicationContextExtensions
    {
        public static bool IsDependsOnRegistryCenterModule(this ApplicationContext applicationContext,
            out string registryCenterType)
        {
            registryCenterType = EngineContext.Current.Configuration.GetValue<string>("RegistryCenter:Type");
            if (registryCenterType.IsNullOrEmpty())
            {
                throw new SilkyException("You must specify a service registry module");
            }
            
            return applicationContext.IsDependsOnModule(registryCenterType);
        }
    }
}