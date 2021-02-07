using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceExecutor : ITransientDependency
    {
        Task<object> Execute(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters);
    }
}