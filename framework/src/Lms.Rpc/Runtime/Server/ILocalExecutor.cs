using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server
{
    public interface ILocalExecutor 
    {
        object Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null);
    }
}