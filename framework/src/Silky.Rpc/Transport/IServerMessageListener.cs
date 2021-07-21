using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Transport
{
    public interface IServerMessageListener : IMessageListener, IDisposable
    {
        Task Listen();
    }
}