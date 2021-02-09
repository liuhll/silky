using System;
using System.Threading.Tasks;

namespace Lms.Rpc.Transport
{
    public interface IServerMessageListener : IMessageListener, IDisposable
    {
        Task Listen();
    }
}