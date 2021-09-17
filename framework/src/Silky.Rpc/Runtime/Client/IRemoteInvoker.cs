using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRemoteInvoker : IScopedDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage, AddressSelectorMode shuntStrategy,
            string hashKey = null);
    }
}