using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IFallbackInvoker<T> : ITransientDependency
    {
        Task<T> Invoke(IDictionary<string, object> parameters);
    }
}