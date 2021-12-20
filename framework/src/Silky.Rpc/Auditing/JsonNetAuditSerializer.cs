using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Auditing;

public class JsonNetAuditSerializer : IAuditSerializer
{
    protected AuditingOptions Options;

    public JsonNetAuditSerializer(IOptions<AuditingOptions> options)
    {
        Options = options.Value;
    }

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, GetSharedJsonSerializerSettings());
    }
    
    public T Deserialize<T>(string line)
    {
        return JsonConvert.DeserializeObject<T>(line, GetSharedJsonSerializerSettings());
    }

    private static readonly object SyncObj = new object();
    private static JsonSerializerSettings _sharedJsonSerializerSettings;

    private JsonSerializerSettings GetSharedJsonSerializerSettings()
    {
        if (_sharedJsonSerializerSettings == null)
        {
            lock (SyncObj)
            {
                if (_sharedJsonSerializerSettings == null)
                {
                    _sharedJsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new AuditingContractResolver(Options.IgnoredTypes)
                    };
                }
            }
        }

        return _sharedJsonSerializerSettings;
    }
}