using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client;

public interface IRemoteInvoker
{
    Task InvokeAsync();

    RemoteResultMessage RemoteResult { get; }
}