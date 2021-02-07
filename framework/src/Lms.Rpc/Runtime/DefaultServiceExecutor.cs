using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        private readonly IRemoteServiceExecutor _remoteServiceExecutor;

        public DefaultServiceExecutor(IRemoteServiceExecutor remoteServiceExecutor)
        {
            _remoteServiceExecutor = remoteServiceExecutor;
        }

        public async Task<object> Execute([NotNull] ServiceEntry serviceEntry,
            [NotNull] IDictionary<ParameterFrom, object> parameters)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(parameters, nameof(parameters));
            if (serviceEntry.IsLocal)
            {
                var result = await serviceEntry.Executor(null, parameters);
                return result;
            }
            
            return await _remoteServiceExecutor.Execute(serviceEntry, parameters);
        }
    }
}