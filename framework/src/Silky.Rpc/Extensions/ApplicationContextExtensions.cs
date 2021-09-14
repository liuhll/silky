using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core.Modularity;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Extensions
{
    public static class ApplicationContextExtensions
    {
        public static bool IsDependsOnRegistryCenterModule(this ApplicationContext applicationContext,
            out RegistryCenterType registryCenterType)
        {
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetRequiredService<IOptions<RegistryCenterOptions>>().Value;
            registryCenterType = registryCenterOptions.RegistryCenterType;
            return applicationContext.IsDependsOnModule(registryCenterOptions.RegistryCenterType.ToString());
        }
    }
}