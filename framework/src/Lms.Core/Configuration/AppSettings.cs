using System.Collections.Generic;

namespace Lms.Core.Configuration
{
    public partial class AppSettings
    {
        private Dictionary<string, IConfig> _configs = new Dictionary<string, IConfig>();
        
        public T GetConfig<T>(string name) where T : IConfig
        {
            if (_configs.ContainsKey(name))
            {
                return (T)_configs[name];
            }
            return default(T);
        }
    }
}