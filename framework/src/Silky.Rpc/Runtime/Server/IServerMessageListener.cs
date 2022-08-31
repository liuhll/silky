using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerMessageListener : IMessageListener, IDisposable
    {
        Task Listen();
    }
}