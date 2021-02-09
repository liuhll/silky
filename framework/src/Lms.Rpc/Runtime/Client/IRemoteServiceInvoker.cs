using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceInvoker : IScopedDependency
    {
        Task<RemoteResultMessage> Invoke(RemoteInvokeMessage remoteInvokeMessage);
    }
}