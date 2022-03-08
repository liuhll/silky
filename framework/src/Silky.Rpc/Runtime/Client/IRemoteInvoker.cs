using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRemoteInvoker : ISingletonDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage, ShuntStrategy shuntStrategy,
            string hashKey = null);
    }
}