using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Monitor.Provider;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Monitor.Handle
{
    public class DefaultServerHandleMonitor : IServerHandleMonitor
    {
        private readonly IMonitorProvider _monitorProvider;

        public DefaultServerHandleMonitor(IMonitorProvider monitorProvider)
        {
            _monitorProvider = monitorProvider;
        }

        public ServerHandleInfo Monitor((string, string) item)
        {
            lock (_monitorProvider.InstanceHandleInfo)
            {
                _monitorProvider.InstanceHandleInfo.ConcurrentCount++;
                if (_monitorProvider.InstanceHandleInfo.ConcurrentCount >
                    _monitorProvider.InstanceHandleInfo.MaxConcurrentCount)
                {
                    _monitorProvider.InstanceHandleInfo.MaxConcurrentCount =
                        _monitorProvider.InstanceHandleInfo.ConcurrentCount;
                }

                _monitorProvider.InstanceHandleInfo.FirstHandleTime ??= DateTime.Now;
                _monitorProvider.InstanceHandleInfo.FinalHandleTime = DateTime.Now;
                _monitorProvider.InstanceHandleInfo.TotalHandleCount++;
            }

            var serverHandleInfo = _monitorProvider.GetServerHandleInfo(GetCacheKey(item));
            serverHandleInfo.ServiceEntryId = item.Item1;
            serverHandleInfo.Address = item.Item2;
            serverHandleInfo.TotalHandleCount++;
            serverHandleInfo.FinalHandleTime = DateTime.Now;
            return serverHandleInfo;
        }


        public void ExecSuccess((string, string) item, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo)
        {
            lock (_monitorProvider.InstanceHandleInfo)
            {
                _monitorProvider.InstanceHandleInfo.ConcurrentCount--;
                if (elapsedTotalMilliseconds > 0)
                {
                    _monitorProvider.InstanceHandleInfo.AET = _monitorProvider.InstanceHandleInfo.AET.HasValue
                        ? (_monitorProvider.InstanceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            if (elapsedTotalMilliseconds > 0)
            {
                serverHandleInfo.AET = serverHandleInfo.AET.HasValue
                    ? (serverHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _monitorProvider.SetServerHandleInfo(GetCacheKey(item), serverHandleInfo);
        }

        public void ExecFail((string, string) item, bool isSeriousError, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo)
        {
            lock (_monitorProvider.InstanceHandleInfo)
            {
                _monitorProvider.InstanceHandleInfo.ConcurrentCount--;
                _monitorProvider.InstanceHandleInfo.FaultHandleCount++;
                _monitorProvider.InstanceHandleInfo.FinalFaultHandleTime = DateTime.Now;

                if (isSeriousError)
                {
                    _monitorProvider.InstanceHandleInfo.TotalSeriousErrorCount++;
                    _monitorProvider.InstanceHandleInfo.FinalSeriousErrorTime = DateTime.Now;
                }

                if (elapsedTotalMilliseconds > 0)
                {
                    _monitorProvider.InstanceHandleInfo.AET = _monitorProvider.InstanceHandleInfo.AET.HasValue
                        ? (_monitorProvider.InstanceHandleInfo.AET + elapsedTotalMilliseconds) / 2
                        : elapsedTotalMilliseconds;
                }
            }

            serverHandleInfo.FaultHandleCount++;
            serverHandleInfo.FinalSeriousErrorTime = DateTime.Now;
            if (isSeriousError)
            {
                serverHandleInfo.SeriousErrorCount++;
                serverHandleInfo.SeriousErrorTime = DateTime.Now;
            }

            if (elapsedTotalMilliseconds > 0)
            {
                serverHandleInfo.AET = serverHandleInfo.AET.HasValue
                    ? (serverHandleInfo.AET + elapsedTotalMilliseconds) / 2
                    : elapsedTotalMilliseconds;
            }

            _monitorProvider.SetServerHandleInfo(GetCacheKey(item), serverHandleInfo);
        }

        public Task<ServerInstanceHandleInfo> GetServerInstanceHandleInfo()
        {
            return _monitorProvider.GetInstanceHandleInfo();
        }

        public async Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos()
        {
            var serverHandleList = await _monitorProvider.GetServiceEntryHandleInfos();
            return serverHandleList;
        }

        private string GetCacheKey((string, string) item)
        {
            var cacheKey =
                $"HandleSupervisor:{RpcContext.Context.Connection.LocalAddress}:{item.Item1}:{item.Item2}";
            return cacheKey;
        }
    }
}