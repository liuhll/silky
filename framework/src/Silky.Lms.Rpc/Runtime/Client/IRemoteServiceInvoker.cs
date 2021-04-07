using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceInvoker : IScopedDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage,
            GovernanceOptions governanceOptions, string hashKey = null);
    }
}