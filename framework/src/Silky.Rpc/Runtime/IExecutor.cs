using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime
{
    public interface IExecutor
    {
        Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null);

        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<ParameterFrom, object> parameters, string serviceKey);
        
        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters,
            string serviceKey = null);

        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<string, object> parameters,
            string serviceKey = null);
    }
}