using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRemoteExecutor : IExecutor
    {
        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters, string serviceKey = null);
    }
}