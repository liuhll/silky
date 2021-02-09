using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        public async Task<object> Execute([NotNull] ServiceEntry serviceEntry,
            [NotNull] IDictionary<ParameterFrom, object> requestParameters)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(requestParameters, nameof(requestParameters));
            var parameters = serviceEntry.ResolveParameters(requestParameters);
            return await serviceEntry.Executor(null, parameters);
        }
    }
}