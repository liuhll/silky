using System.Collections.Generic;

namespace Silky.Rpc.RegistryCenters
{
    public interface IRegisterCenterHealthProvider
    {
        IDictionary<string, RegistryCenterHealthCheckModel> GetRegistryCenterHealthInfo();
    }
}