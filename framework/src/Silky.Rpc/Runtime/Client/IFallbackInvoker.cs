using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IFallbackInvoker : IScopedDependency
    {
        Task<object> Invoke(ServiceEntry serviceEntry, object[] parameters);
    }
}