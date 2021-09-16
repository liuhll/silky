using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal class InnerHttpMessageReceivedHandler : MessageReceivedHandlerBase
    {
        public InnerHttpMessageReceivedHandler(IOptionsMonitor<RpcOptions> rpcOptions, IExecutor executor) : base(
            rpcOptions, executor)

        {
        }

        public override Task Handle(ServiceEntry serviceEntry)
        {
            throw new SilkyException("rpc communication does not support http protocol temporarily");
        }

        protected override Task HandleResult(object result)
        {
            throw new NotImplementedException();
        }

        protected override Task HandleException(Exception exception)
        {
            throw new NotImplementedException();
        }

        protected override Task<string> ResolveServiceKey()
        {
            throw new NotImplementedException();
        }

        protected override Task<object[]> ResolveParameters(ServiceEntry serviceEntry)
        {
            throw new NotImplementedException();
        }
    }
}