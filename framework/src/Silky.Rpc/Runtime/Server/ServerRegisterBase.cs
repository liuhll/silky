using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public abstract class ServerRegisterBase : IServerRegister
    {
        protected readonly IServerManager _serverManager;
        protected readonly IServerProvider _serverProvider;
        protected RpcOptions _rpcOptions;
        public ILogger<ServerRegisterBase> Logger { get; set; }

        protected ServerRegisterBase(IServerManager serverManager,
            IServerProvider serverProvider,
            IOptionsMonitor<RpcOptions> rpcOptions)
        {
            _serverManager = serverManager;
            _serverProvider = serverProvider;


            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);
            Check.NotNullOrEmpty(_rpcOptions.Token, nameof(_rpcOptions.Token));
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

        public abstract Task RemoveSelfServer();

        protected abstract Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint);

        protected abstract Task CacheServers();

        protected abstract Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor);

        protected abstract Task RemoveServiceCenterExceptRpcEndpoint(IServer server);
    }
}