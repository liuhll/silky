using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface ILocalExecutor
    {
        Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null,
            MethodType methodType = MethodType.Try);
    }
}