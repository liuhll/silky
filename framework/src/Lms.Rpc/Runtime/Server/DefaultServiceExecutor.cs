using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;

namespace Lms.Rpc.Runtime.Server
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        public async Task<object> Execute([NotNull] ServiceEntry.ServiceEntry serviceEntry,[NotNull]IDictionary<ParameterFrom, object> parameters)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(parameters, nameof(parameters));
            if (serviceEntry.IsLocal)
            {
                var result = await serviceEntry.Executor(null, parameters);
                return result;
            }
        
            return null;
        }
       
    }
}