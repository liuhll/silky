using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    internal interface IRemoteCaller : IScopedDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage, ShuntStrategy shuntStrategy,
            string hashKey = null);
    }
}