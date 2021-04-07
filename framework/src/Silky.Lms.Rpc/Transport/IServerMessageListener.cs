using System;
using System.Threading.Tasks;

namespace Silky.Lms.Rpc.Transport
{
    public interface IServerMessageListener : IMessageListener, IDisposable
    {
        Task Listen();
    }
}