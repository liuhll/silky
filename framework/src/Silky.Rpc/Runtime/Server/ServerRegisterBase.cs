using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public abstract class ServerRegisterBase : IServerRegister
    {
        protected readonly IServerManager _serverManager;
        protected readonly IServerProvider _serverProvider;
        protected IServer _server;

        public ILogger<ServerRegisterBase> Logger { get; set; }

        protected ServerRegisterBase(IServerManager serverManager,
            IServerProvider serverProvider)
        {
            _serverManager = serverManager;
            _serverProvider = serverProvider;
            Logger = NullLogger<ServerRegisterBase>.Instance;
            _serverManager.OnRemoveRpcEndpoint += async (hostName, rpcEndpoint) =>
            {
                await RemoveSilkyEndpoint(hostName, rpcEndpoint);
            };
        }

        public virtual async Task RegisterServer()
        {
            _server = _serverProvider.GetServer();
            await RemoveServiceCenterExceptRpcEndpoint(_server);
            await RegisterServerToServiceCenter(_server.ConvertToDescriptor());
            await CacheServers();
        }

        protected virtual async Task<bool> RepeatRegister()
        {
            var selfServerInfo = _serverManager.GetSelfServer();
            var localServer = _serverProvider.GetServer();
            var needRegister = false;
            foreach (var endpoint in localServer.Endpoints)
            {
                if (!selfServerInfo.Endpoints.Any(e => e.Equals(endpoint)))
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
            var tcpEndpoint = _server.RpcEndpoint;
            if (tcpEndpoint != null)
            {
                await RemoveSilkyEndpoint(EngineContext.Current.HostName, tcpEndpoint);
            }

            var wsEndpoint = _server.WsEndpoint;
            if (wsEndpoint != null)
            {
                await RemoveSilkyEndpoint(EngineContext.Current.HostName, wsEndpoint);
            }

            var httpEndpoint = _server.WebEndpoint;
            if (httpEndpoint != null)
            {
                await RemoveSilkyEndpoint(EngineContext.Current.HostName, httpEndpoint);
            }
        }

        protected abstract Task RemoveSilkyEndpoint(string hostName, ISilkyEndpoint silkyEndpoint);

        protected abstract Task CacheServers();

        protected abstract Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor);

        protected abstract Task RemoveServiceCenterExceptRpcEndpoint(IServer server);
    }
}