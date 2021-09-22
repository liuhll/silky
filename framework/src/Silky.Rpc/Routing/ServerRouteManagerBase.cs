using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Microsoft.Extensions.Options;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public abstract class ServerRouteManagerBase : IServerRouteManager
    {
        protected readonly ServerRouteCache _serverRouteCache;
        protected readonly IServerRegisterProvider _serverRegisterProvider;
        protected RegistryCenterOptions _registryCenterOptions;
        protected RpcOptions _rpcOptions;
        public ILogger<ServerRouteManagerBase> Logger { get; set; }

        protected ServerRouteManagerBase(ServerRouteCache serverRouteCache,
            IServerRegisterProvider serverRegisterProvider,
            IOptionsMonitor<RegistryCenterOptions> registryCenterOptions,
            IOptionsMonitor<RpcOptions> rpcOptions)
        {
            _serverRouteCache = serverRouteCache;
            _serverRegisterProvider = serverRegisterProvider;
            _registryCenterOptions = registryCenterOptions.CurrentValue;

            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);

            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath, nameof(_registryCenterOptions.RoutePath));
            Check.NotNullOrEmpty(_rpcOptions.Token, nameof(_rpcOptions.Token));
            Logger = NullLogger<ServerRouteManagerBase>.Instance;
            _serverRouteCache.OnRemoveRpcEndpoint += async (hostName, rpcEndpoint) =>
            {
                await RemoveRpcEndpoint(hostName, rpcEndpoint);
            };
        }

        public virtual async Task RegisterServerRoute()
        {
            var serverRoute = _serverRegisterProvider.GetServerRoute();
            await RemoveServiceCenterExceptRpcEndpoint(serverRoute);
            await RegisterServerRouteToServiceCenter(serverRoute.ConvertToDescriptor());
            await EnterRoutes();
        }

        protected abstract Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint);

        protected abstract Task EnterRoutes();

        protected abstract Task RegisterServerRouteToServiceCenter(ServerRouteDescriptor serverRouteDescriptor);

        protected abstract Task RemoveServiceCenterExceptRpcEndpoint(ServerRoute serverRoute);
    }
}