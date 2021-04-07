using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Runtime
{
    public interface IServiceExecutor
    {
        Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null);
    }
}