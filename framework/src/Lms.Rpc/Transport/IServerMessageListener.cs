using System.Threading.Tasks;

namespace Lms.Rpc.Transport
{
    public interface IServerMessageListener : IMessageListener
    {
        Task Listen();
    }
}