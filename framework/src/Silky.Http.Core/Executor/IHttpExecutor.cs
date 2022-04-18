using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Executor
{
    public interface IHttpExecutor : IExecutor
    {
        Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<ParameterFrom, object> parameters, string serviceKey);
    }
}