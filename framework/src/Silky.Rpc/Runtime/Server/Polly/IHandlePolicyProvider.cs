using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandlePolicyProvider : IScopedDependency
    {
        IAsyncPolicy Create(RemoteInvokeMessage message);
    }
}