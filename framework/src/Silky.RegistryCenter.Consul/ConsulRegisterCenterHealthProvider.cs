using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Utils;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulRegisterCenterHealthProvider : IRegisterCenterHealthProvider
    {
        private readonly ConsulRegistryCenterOptions _consulRegistryCenterOptions;

        public ConsulRegisterCenterHealthProvider(IOptions<ConsulRegistryCenterOptions> consulRegistryCenterOptions)
        {
            _consulRegistryCenterOptions = consulRegistryCenterOptions.Value;
        }

        public IDictionary<string, RegistryCenterHealthCheckModel> GetRegistryCenterHealthInfo()
        {
            var registryCenterHealthResult = new Dictionary<string, RegistryCenterHealthCheckModel>();
            var consulServerAddress = _consulRegistryCenterOptions.Address.ToString();

            if (UrlCheck.UrlIsValid(consulServerAddress, out var exMessage))
            {
                registryCenterHealthResult.Add(consulServerAddress, new RegistryCenterHealthCheckModel(true));
            }
            else
            {
                registryCenterHealthResult.Add(consulServerAddress, new RegistryCenterHealthCheckModel(false)
                {
                    UnHealthReason = exMessage,
                    HealthType = HealthType.Disconnected
                });
            }

            return registryCenterHealthResult;
        }
    }
}