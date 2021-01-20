using Newtonsoft.Json;

namespace Lms.Core.Configuration
{
    public interface IConfig
    {
        [JsonIgnore]
        string Name => GetType().Name;
    }
}