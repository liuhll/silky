using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerRegister
    {
        Task RegisterServer();

        Task RemoveSelf();
    }
}