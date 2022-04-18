using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    internal interface IRemoteExecutor : IExecutor
    {
        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters,
            string serviceKey = null);

        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<string, object> parameters,
            string serviceKey = null);
        
        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<ParameterFrom, object> parameters,
            string serviceKey = null);
    }
}