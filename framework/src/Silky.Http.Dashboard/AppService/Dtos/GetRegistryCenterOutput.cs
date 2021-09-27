using Silky.Rpc.RegistryCenters;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetRegistryCenterOutput : RegistryCenterHealthCheckModel
    {
        public string RegistryCenterType { get; set; }

        public string RegistryCenterAddress { get; set; }
    }
}