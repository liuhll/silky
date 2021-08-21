using Silky.Rpc.Configuration;
using Silky.Rpc.RegistryCenters;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetRegistryCenterOutput : RegistryCenterHealthCheckModel
    {
        public RegistryCenterType RegistryCenterType { get; set; }

        public string RegistryCenterAddress { get; set; }
    }
}