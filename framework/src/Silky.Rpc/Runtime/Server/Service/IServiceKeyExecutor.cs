using System;
using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceKeyExecutor
    {
        string ServiceKey { get; }

        Task Execute(Func<Task> func, string serviceKey);

        Task<T> Execute<T>(Func<Task<T>> func, string serviceKey);
    }
}