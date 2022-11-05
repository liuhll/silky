using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Rpc.Endpoint;
using static Silky.Rpc.Endpoint.SilkyEndpointHelper;

namespace Silky.Rpc.Runtime.Server
{
    public abstract class ServerRegisterBase : IServerRegister
    {
        protected readonly IServerManager _serverManager;
        protected readonly IServerProvider _serverProvider;
        public ILogger<ServerRegisterBase> Logger { get; set; }

        protected ServerRegisterBase(IServerManager serverManager,
            IServerProvider serverProvider)
        {
            _serverManager = serverManager;
            _serverProvider = serverProvider;
            Logger = NullLogger<ServerRegisterBase>.Instance;
            _serverManager.OnRemoveRpcEndpoint += async (hostName, rpcEndpoint) =>
            {
                await RemoveRpcEndpoint(hostName, rpcEndpoint);
            };
        }

        public virtual async Task RegisterServer()
        {
            var server = _serverProvider.GetServer();
            await RemoveServiceCenterExceptRpcEndpoint(server);
            await RegisterServerToServiceCenter(server.ConvertToDescriptor());
            await CacheServers();
        }

        protected virtual async Task<bool> RepeatRegister()
        {
            var selfServerInfo = _serverManager.GetSelfServer();
            var localServer = _serverProvider.GetServer();
            var needRegister = false;
            foreach (var endpoint in localServer.Endpoints)
            {
                if (!selfServerInfo.Endpoints.Any(e=> e.Equals(endpoint)))
                {
                    needRegister = true;
                    break;
                }
            }

            if (needRegister)
            {
                await RegisterServer();
            }

            return needRegister;
        }

        public virtual async Task RemoveSelf()
        {
            if (EngineContext.Current.IsContainDotNettyTcpModule())
            {
                var tcpEndpoint = GetLocalRpcEndpoint();
                await RemoveRpcEndpoint(EngineContext.Current.HostName, tcpEndpoint);
            }

            if (EngineContext.Current.IsContainWebSocketModule())
            {
                var wsEndpoint = GetWsEndpoint();
                await RemoveRpcEndpoint(EngineContext.Current.HostName, wsEndpoint);
            }

            if (EngineContext.Current.IsContainHttpCoreModule())
            {
                var httpEndpoint = GetLocalWebEndpoint();
                if (httpEndpoint != null)
                {
                    await RemoveRpcEndpoint(EngineContext.Current.HostName, httpEndpoint);
                }
            }
        }

        protected abstract Task RemoveRpcEndpoint(string hostName, ISilkyEndpoint silkyEndpoint);

        protected abstract Task CacheServers();

        protected abstract Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor);

        protected abstract Task RemoveServiceCenterExceptRpcEndpoint(IServer server);
    }
}