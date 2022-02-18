using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.HealthChecks.Rpc
{
    public class SilkyRpcHealthCheck : IHealthCheck
    {
        private readonly IServerManager _serverManager;
        private readonly IServerHealthCheck _serverHealthCheck;
        private readonly ICurrentRpcToken _currentRpcToken;
        private readonly ISerializer _serializer;
        public SilkyRpcHealthCheck(IServerManager serverManager,
            IServerHealthCheck serverHealthCheck,
            ICurrentRpcToken currentRpcToken,
            ISerializer serializer)
        {
            _serverManager = serverManager;
            _serverHealthCheck = serverHealthCheck;
            _currentRpcToken = currentRpcToken;
            _serializer = serializer;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var servers = _serverManager.Servers;
            var healthData = new Dictionary<string, object>();
            
            _currentRpcToken.SetRpcToken();
            foreach (var server in servers)
            {
                foreach (var endpoint in server.Endpoints)
                {
                    if (endpoint.ServiceProtocol == ServiceProtocol.Tcp)
                    {
                        var endpointHealthData = new ServerHealthData()
                        {
                            HostName = server.HostName,
                            Address = endpoint.GetAddress(),
                            ServiceProtocol = endpoint.ServiceProtocol,
                        };
                        bool isHealth;
                        try
                        {
                            isHealth = await _serverHealthCheck.IsHealth(endpoint);
                        }
                        catch (Exception e)
                        {
                            isHealth = false;
                        }

                        endpointHealthData.Health = isHealth;
                        healthData[endpoint.GetAddress()] = endpointHealthData;
                    }
                }
            }
            var serverHealthList = healthData.Values.Select(p=> (ServerHealthData)p);
            var serverHealthGroups = serverHealthList.GroupBy(p => p.HostName);
            var healthCheckDescriptions = new Dictionary<string, object>();
            foreach (var serverHealthGroup in serverHealthGroups)
            {
                var serverDesc = new List<string>();
                var healthCount = serverHealthGroup.Count(p => p.Health);
                if (healthCount > 0)
                {
                    serverDesc.Add($"HealthCount:{healthCount}");
                }
                var unHealthCount = serverHealthGroup.Count(p => !p.Health);
                if (unHealthCount > 0)
                {
                    serverDesc.Add($"UnHealthCount:{unHealthCount}");
                }

                healthCheckDescriptions[serverHealthGroup.Key] = serverDesc;
            }
            
            var detail = _serializer.Serialize(healthCheckDescriptions,false);
            if (healthData.Values.All(p => ((ServerHealthData)p).Health))
            {
                return HealthCheckResult.Healthy(
                    $"There are a total of {healthData.Count} Rpc service provider instances." +
                    $"{Environment.NewLine} server detail:{detail}.",
                    healthData);
            }

            if (healthData.Values.All(p => !((ServerHealthData)p).Health))
            {
                return HealthCheckResult.Unhealthy(
                    $"There are a total of {healthData.Count} Rpc service provider instances, and all service provider instances are unhealthy." +
                    $"{Environment.NewLine} server detail:{detail}.",
                    null, healthData);
            }

            var unHealthData = healthData.Values.Where(p => !((ServerHealthData)p).Health)
                .Select(p => (ServerHealthData)p).ToArray();

            return HealthCheckResult.Degraded(
                $"There are a total of {healthData.Count}  Rpc service provider instances," +
                $" of which {unHealthData.Count()}" +
                $" service instances are unhealthy{Environment.NewLine}." +
                $" unhealthy instances:{string.Join(",", unHealthData.Select(p => p.Address))}." +
                $" server detail:{detail}.",
                null, healthData);
        }
        
    }
}