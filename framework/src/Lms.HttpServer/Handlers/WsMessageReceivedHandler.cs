using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Serialization;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Configuration;
using Lms.Rpc.Messages;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer.Handlers
{
    internal class WsMessageReceivedHandler : MessageReceivedHandlerBase, IWsShakeHandHandler
    {
        private readonly IRemoteServiceInvoker _remoteServiceInvoker;
        private readonly GovernanceOptions _governanceOptions;

        public WsMessageReceivedHandler(IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IServiceExecutor serviceExecutor,
            IRemoteServiceInvoker remoteServiceInvoker,
            IOptions<GovernanceOptions> governanceOptions)
            : base(parameterParser,
                serializer,
                rpcOptions,
                serviceExecutor)
        {
            _governanceOptions = governanceOptions.Value;
            _governanceOptions.ShuntStrategy = AddressSelectorMode.HashAlgorithm;
            _remoteServiceInvoker = remoteServiceInvoker;
        }

        public async Task Connection(ServiceRoute serviceRoute, HttpContext context)
        {
            if (context.Request.Headers.Any())
            {
                var headerData = context.Request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString());
                RpcContext.GetContext().SetAttachment("requestHeader", headerData);
            }

            var hashKey = context.Request.Query["hashKey"].ToString();
            RpcContext.GetContext().SetAttachment("hashKey", hashKey);
            RpcContext.GetContext().SetAttachment("uri",context.Request.Path);
            var remoteMessage = new RemoteInvokeMessage()
            {
                ServiceId = serviceRoute.ServiceDescriptor.Id,
                Parameters = new object[0]
            };
            await _remoteServiceInvoker.Invoke(remoteMessage, _governanceOptions, hashKey);
        }
    }
}