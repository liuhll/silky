using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client;

public interface IInvokeTemplate : IScopedDependency
{
    Task<T> GetForObjectAsync<T>(string api, params object[] parameters);

    Task<T> GetForObjectAsync<T>(string api, IDictionary<string, object> parameters);

    Task<T> PostForObjectAsync<T>(string api, params object[] parameters);

    Task<T> PostForObjectAsync<T>(string api, IDictionary<string, object> parameters);

    Task<T> PutForObjectAsync<T>(string api, params object[] parameters);

    Task<T> PutForObjectAsync<T>(string api, IDictionary<string, object> parameters);

    Task<T> PatchForObjectAsync<T>(string api, params object[] parameters);

    Task<T> PatchForObjectAsync<T>(string api, IDictionary<string, object> parameters);

    Task<T> DeleteForObjectAsync<T>(string api, params object[] parameters);

    Task<T> DeleteForObjectAsync<T>(string api, IDictionary<string, object> parameters);

    Task<T> InvokeForObjectByServiceEntryId<T>(string serviceEntryId, params object[] parameters);

    Task<T> InvokeForObjectByServiceEntryId<T>(string serviceEntryId, IDictionary<string, object> parameters);

    Task<T> InvokeForObjectByWebApi<T>(string api, HttpMethod httpMethod, params object[] parameters);

    Task<T> InvokeForObjectByWebApi<T>(string api, HttpMethod httpMethod, IDictionary<string, object> parameters);

    Task GetAsync(string api, params object[] parameters);

    Task GetAsync(string api, IDictionary<string, object> parameters);

    Task PostAsync(string api, params object[] parameters);

    Task PostAsync(string api, IDictionary<string, object> parameters);

    Task PutAsync(string api, params object[] parameters);

    Task PutAsync(string api, IDictionary<string, object> parameters);

    Task PatchAsync(string api, params object[] parameters);

    Task PatchAsync(string api, IDictionary<string, object> parameters);

    Task DeleteAsync(string api, params object[] parameters);

    Task DeleteAsync(string api, IDictionary<string, object> parameters);

    Task InvokeByServiceEntryId(string serviceEntryId, params object[] parameters);

    Task InvokeByServiceEntryId(string serviceEntryId, IDictionary<string, object> parameters);
    
    Task InvokeByWebApi(string api, HttpMethod httpMethod, params object[] parameters);

    Task InvokeByWebApi(string api, HttpMethod httpMethod, IDictionary<string, object> parameters);
}