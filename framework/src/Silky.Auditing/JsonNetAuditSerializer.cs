using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Silky.Auditing.Configuration;

namespace Silky.Auditing;

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