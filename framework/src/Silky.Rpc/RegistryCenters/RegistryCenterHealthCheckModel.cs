namespace Silky.Rpc.RegistryCenters
{
    public class RegistryCenterHealthCheckModel
    {
        public RegistryCenterHealthCheckModel()
        {
        }

        public RegistryCenterHealthCheckModel(bool isHealth)
        {
            IsHealth = isHealth;
        }

        public RegistryCenterHealthCheckModel(bool isHealth, int unHealthTimes)
        {
            IsHealth = isHealth;
            UnHealthTimes = unHealthTimes;
        }

        public bool IsHealth { get; set; }

        public int UnHealthTimes { get; set; }

        public string UnHealthReason { get; set; }
    }
}