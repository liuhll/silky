using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerMessageListener : IMessageListener, IAsyncDisposable
    {
        Task Listen();
    }
}