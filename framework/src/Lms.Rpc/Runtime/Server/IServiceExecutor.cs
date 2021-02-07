using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;

namespace Lms.Rpc.Runtime.Server
{
    public interface IServiceExecutor : ITransientDependency
    {
         Task<object> Execute(ServiceEntry.ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters);
    }
}