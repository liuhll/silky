using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandlePolicyWithResultProvider : IScopedDependency
    {
        IAsyncPolicy<RemoteResultMessage> Create(RemoteInvokeMessage message);
    }
}