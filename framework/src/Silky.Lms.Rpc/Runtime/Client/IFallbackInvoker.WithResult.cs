using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Runtime.Client
{
    public interface IFallbackInvoker<T> : ITransientDependency
    {
        Task<T> Invoke(IDictionary<string, object> parameters);
    }
}