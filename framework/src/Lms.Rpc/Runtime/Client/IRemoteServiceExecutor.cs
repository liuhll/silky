using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceExecutor : ITransientDependency
    {
        Task<object> Execute(string serviceId, IList<object> parameters);
        
        Task<object> Execute(ServiceEntry serviceEntry, IList<object> parameters);
    }
}