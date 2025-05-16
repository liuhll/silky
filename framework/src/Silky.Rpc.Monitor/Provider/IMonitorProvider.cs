using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Monitor.Provider;

public interface IMonitorProvider
{
    ClientInvokeInfo GetInvokeInfo(string cacheKey);

    ServerInstanceInvokeInfo InstanceInvokeInfo { get; }
    
    ServerInstanceHandleInfo InstanceHandleInfo { get; }
    
    Task<ServerInstanceInvokeInfo> GetInstanceInvokeInfo();
    
    Task<ServerInstanceHandleInfo> GetInstanceHandleInfo();
    
    void SetClientInvokeInfo(string cacheKey, ClientInvokeInfo clientInvokeInfo);
    
    Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos();
    
    Task ClearClientInvokeCache();
    
    ServerHandleInfo? GetServerHandleInfo(string cacheKey);
    
    void SetServerHandleInfo(string cacheKey, ServerHandleInfo? serverHandleInfo);
    
    Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos();
    Task ClearServerHandleCache();

    
}