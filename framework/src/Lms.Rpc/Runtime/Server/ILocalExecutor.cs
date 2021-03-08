using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server
{
    public interface ILocalExecutor : IScopedDependency
    {
        object Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null);
    }
}