using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Configuration;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRemoteInvoker : IScopedDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            GovernanceOptions governanceOptions, string hashKey = null);
    }
}