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
            return await Execute(serviceEntry, parameters, null);
        }

        public async Task<object> Execute([NotNull] ServiceEntry serviceEntry, [NotNull] IList<object> parameters,
            [CanBeNull] string serviceKey = null)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(parameters, nameof(parameters));
            return await serviceEntry.Executor(serviceKey, parameters);
        }
    }
}