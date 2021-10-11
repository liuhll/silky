using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IAppointAddressInvoker : IScopedDependency
    {
        Task<object> Invoke([NotNull] string address, [NotNull] ServiceEntry serviceEntry,
            object[] parameters, string serviceKey = null);

        Task<T> Invoke<T>([NotNull] string address, [NotNull] ServiceEntry serviceEntry, object[] parameters,
            string serviceKey = null);

        Task<object> Invoke([NotNull] string address, [NotNull] string serviceEntryId,
            object[] parameters, string serviceKey = null);

        Task<T> Invoke<T>([NotNull] string address, [NotNull] string serviceEntryId, object[] parameters,
            string serviceKey = null);
    }
}