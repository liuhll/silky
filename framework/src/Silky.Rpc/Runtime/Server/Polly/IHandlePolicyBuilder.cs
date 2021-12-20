using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandlePolicyBuilder : ISingletonDependency
    {
        IAsyncPolicy<RemoteResultMessage> Build(RemoteInvokeMessage message);
    }
}