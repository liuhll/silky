using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client;

internal class DefaultInvokeTemplate : IInvokeTemplate
{
    private readonly IExecutor _executor;
    private readonly IServerManager _serverManager;
    private readonly ITypeConvertibleService _typeConvertibleService;


    public DefaultInvokeTemplate(IExecutor executor,
        IServerManager serverManager,
        ITypeConvertibleService typeConvertibleService)
    {
        _executor = executor;
        _serverManager = serverManager;
        _typeConvertibleService = typeConvertibleService;
    }

    public async Task<T> GetForObjectAsync<T>(string api, params object[] parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Get, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> GetForObjectAsync<T>(string api, IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Get, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PostForObjectAsync<T>(string api, params object[] parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Post, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PostForObjectAsync<T>(string api, IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Post, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PutForObjectAsync<T>(string api, params object[] parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Put, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PutForObjectAsync<T>(string api, IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Put, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PatchForObjectAsync<T>(string api, params object[] parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Patch, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> PatchForObjectAsync<T>(string api, IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Patch, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> DeleteForObjectAsync<T>(string api, params object[] parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Delete, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> DeleteForObjectAsync<T>(string api, IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, HttpMethod.Delete, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> InvokeForObjectByServiceEntryId<T>(string serviceEntryId, params object[] parameters)
    {
        var result = await InvokeById(serviceEntryId, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> InvokeForObjectByServiceEntryId<T>(string serviceEntryId,
        IDictionary<string, object> parameters)
    {
        var result = await InvokeById(serviceEntryId, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> InvokeForObjectByWebApi<T>(string api, HttpMethod httpMethod, params object[] parameters)
    {
        var result = await InvokeByApi(api, httpMethod, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task<T> InvokeForObjectByWebApi<T>(string api, HttpMethod httpMethod,
        IDictionary<string, object> parameters)
    {
        var result = await InvokeByApi(api, httpMethod, parameters);
        return (T)GetResult(result, typeof(T));
    }

    public async Task GetAsync(string api, params object[] parameters)
    {
        await InvokeByApi(api, HttpMethod.Get, parameters);
    }

    public async Task GetAsync(string api, IDictionary<string, object> parameters)
    {
        await InvokeByApi(api, HttpMethod.Get, parameters);
    }

    public async Task PostAsync(string api, params object[] parameters)
    {
        await InvokeByApi(api, HttpMethod.Post, parameters);
    }

    public async Task PostAsync(string api, IDictionary<string, object> parameters)
    {
        await InvokeByApi(api, HttpMethod.Post, parameters);
    }

    public async Task PutAsync(string api, params object[] parameters)
    {
        await InvokeByApi(api, HttpMethod.Put, parameters);
    }

    public async Task PutAsync(string api, IDictionary<string, object> parameters)
    {
        await InvokeByApi(api, HttpMethod.Put, parameters);
    }

    public async Task PatchAsync(string api, params object[] parameters)
    {
        await InvokeByApi(api, HttpMethod.Patch, parameters);
    }

    public async Task PatchAsync(string api, IDictionary<string, object> parameters)
    {
        await InvokeByApi(api, HttpMethod.Patch, parameters);
    }

    public async Task DeleteAsync(string api, params object[] parameters)
    {
        await InvokeByApi(api, HttpMethod.Delete, parameters);
    }

    public async Task DeleteAsync(string api, IDictionary<string, object> parameters)
    {
        await InvokeByApi(api, HttpMethod.Delete, parameters);
    }

    public Task InvokeByServiceEntryId(string serviceEntryId, params object[] parameters)
    {
        return InvokeById(serviceEntryId, parameters);
    }

    public Task InvokeByServiceEntryId(string serviceEntryId, IDictionary<string, object> parameters)
    {
        return InvokeById(serviceEntryId, parameters);
    }

    public Task InvokeByWebApi(string api, HttpMethod httpMethod, params object[] parameters)
    {
        return InvokeByApi(api, httpMethod, parameters);
    }

    public Task InvokeByWebApi(string api, HttpMethod httpMethod, IDictionary<string, object> parameters)
    {
        return InvokeByApi(api, httpMethod, parameters);
    }


    private async Task<object> InvokeByApi(string api, HttpMethod method, params object[] parameters)
    {
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(api, method);
        if (serviceEntryDescriptor == null)
        {
            throw new NotFindServiceEntryException($"Relevant service entry descriptor not found via {api}-{method}");
        }

        var result = await _executor.Execute(serviceEntryDescriptor, parameters);
        return result;
    }

    private async Task<object> InvokeByApi(string api, HttpMethod method, IDictionary<string, object> parameters)
    {
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(api, method);
        if (serviceEntryDescriptor == null)
        {
            throw new NotFindServiceEntryException($"Relevant service entry descriptor not found via {api}-{method}");
        }

        var result = await _executor.Execute(serviceEntryDescriptor, parameters);
        return result;
    }

    private async Task<object> InvokeById(string id, params object[] parameters)
    {
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(id);
        if (serviceEntryDescriptor == null)
        {
            throw new NotFindServiceEntryException($"Relevant service entry descriptor not found via {id}");
        }

        var result = await _executor.Execute(serviceEntryDescriptor, parameters);
        return result;
    }

    private async Task<object> InvokeById(string id, IDictionary<string, object> parameters)
    {
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(id);
        if (serviceEntryDescriptor == null)
        {
            throw new NotFindServiceEntryException($"Relevant service entry descriptor not found via {id}");
        }

        var result = await _executor.Execute(serviceEntryDescriptor, parameters);
        return result;
    }

    private object GetResult(object result, Type resultType)
    {
        if (result == null)
        {
            return result;
        }


        result = result.GetType() == resultType
            ? result
            : _typeConvertibleService.Convert(result, resultType);

        return result;
    }
}