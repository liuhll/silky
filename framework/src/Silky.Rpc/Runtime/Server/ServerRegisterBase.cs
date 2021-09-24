using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Endpoint;

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

        public abstract Task RemoveSelf();

        protected abstract Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint);

        protected abstract Task CacheServers();

        protected abstract Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor);

        protected abstract Task RemoveServiceCenterExceptRpcEndpoint(IServer server);
    }
}