using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Utils;

namespace Silky.RegistryCenter.Nacos
{
    public class NocasRegisterCenterHealthProvider : IRegisterCenterHealthProvider
    {
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;

        public NocasRegisterCenterHealthProvider(IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions)
        {
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
        }

        public IDictionary<string, RegistryCenterHealthCheckModel> GetRegistryCenterHealthInfo()
        {
            var registryCenterHealthResult = new Dictionary<string, RegistryCenterHealthCheckModel>();
            var nocasServerAddresses = _nacosRegistryCenterOptions.ServerAddresses;
            foreach (var nocasServerAddress in nocasServerAddresses)
            {
                if (UrlCheck.UrlIsValid(nocasServerAddress, out var exMessage))
                {
                    registryCenterHealthResult.Add(nocasServerAddress, new RegistryCenterHealthCheckModel(true));
                }
                else
                {
                    registryCenterHealthResult.Add(nocasServerAddress, new RegistryCenterHealthCheckModel(false)
                    {
                        UnHealthReason = exMessage,
                        HealthType = HealthType.Disconnected
                    });
                }
            }

            return registryCenterHealthResult;
        }

        private (string, int) ParseServerAddress(string nocasServerAddress)
        {
            throw new System.NotImplementedException();
        }
    }
}