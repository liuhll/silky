using System;
using System.Threading.Tasks;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerMessageListener : IMessageListener, IDisposable
    {
        Task Listen();
    }
}