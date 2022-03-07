using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Silky.Core.Exceptions;
using Silky.Core.Serialization;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.HealthChecks.Rpc
{
    public abstract class SilkyHealthCheckBase : IHealthCheck
    {
        protected readonly IServerManager _serverManager;
        protected readonly IServerHealthCheck _serverHealthCheck;
        protected readonly ICurrentRpcToken _currentRpcToken;
        protected readonly ISerializer _serializer;
        protected readonly IHttpHandleDiagnosticListener _httpHandleDiagnosticListener;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IServiceEntryLocator _serviceEntryLocator;

        protected SilkyHealthCheckBase(IServerManager serverManager,
            IServerHealthCheck serverHealthCheck,
            ICurrentRpcToken currentRpcToken,
            ISerializer serializer,
            IHttpHandleDiagnosticListener httpHandleDiagnosticListener,
            IHttpContextAccessor httpContextAccessor,
            IServiceEntryLocator serviceEntryLocator)
        {
            _serverManager = serverManager;
            _serverHealthCheck = serverHealthCheck;
            _currentRpcToken = currentRpcToken;
            _serializer = serializer;
            _httpHandleDiagnosticListener = httpHandleDiagnosticListener;
            _httpContextAccessor = httpContextAccessor;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _currentRpcToken.SetRpcToken();
            _httpContextAccessor.HttpContext.SetHttpHandleAddressInfo();
            var messageId = GetMessageId(_httpContextAccessor.HttpContext);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(HealthCheckConstants.HealthCheckServiceEntryId);
            var tracingTimestamp =
                _httpHandleDiagnosticListener.TracingBefore(messageId, serviceEntry, _httpContextAccessor.HttpContext,
                    Array.Empty<object>());
            try
            {
                var rpcServers = GetServers();

                var healthData = new Dictionary<string, object>();
                foreach (var server in rpcServers)
                {
                    foreach (var endpoint in server.Endpoints)
                    {
                        var endpointHealthData = new ServerHealthData()
                        {
                            HostName = server.HostName,
                            Address = endpoint.GetAddress(),
                            ServiceProtocol = endpoint.ServiceProtocol,
                        };
                        var isHealth = await CheckHealthEndpoint(endpoint);
                        endpointHealthData.Health = isHealth;
                        healthData[endpoint.GetAddress()] = endpointHealthData;
                    }
                }

                var serverHealthList = healthData.Values.Select(p => (ServerHealthData)p);
                var serverHealthGroups = serverHealthList.GroupBy(p => p.HostName);
                var healthCheckDescriptions = new Dictionary<string, object>();
                foreach (var serverHealthGroup in serverHealthGroups)
                {
                    var serverDesc = new List<string>();
                    var healthCount = serverHealthGroup
                        .Count(p => p.Health);
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

                var detail = _serializer.Serialize(healthCheckDescriptions, false);
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
            catch (Exception ex)
            {
                _httpHandleDiagnosticListener.TracingError(tracingTimestamp, messageId, serviceEntry,
                    _httpContextAccessor.HttpContext, ex, StatusCode.ServerError);
                return HealthCheckResult.Unhealthy("health error", ex);
            }
            finally
            {
                _httpHandleDiagnosticListener.TracingAfter(tracingTimestamp, messageId, serviceEntry,
                    _httpContextAccessor.HttpContext, null);
            }
        }

        protected abstract Task<bool> CheckHealthEndpoint(IRpcEndpoint endpoint);
        protected abstract ICollection<IServer> GetServers();

        private string GetMessageId(HttpContext httpContext)
        {
            httpContext.SetHttpMessageId();
            return httpContext.TraceIdentifier;
        }
    }
}