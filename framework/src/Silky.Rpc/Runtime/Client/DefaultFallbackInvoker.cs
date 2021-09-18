using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Rpc;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultFallbackInvoker : IFallbackInvoker
    {
        public ILogger<DefaultFallbackInvoker> Logger { get; set; }

        public DefaultFallbackInvoker()
        {
            Logger = NullLogger<DefaultFallbackInvoker>.Instance;
        }

        public async Task<object> Invoke([NotNull] ServiceEntry serviceEntry, object[] parameters)
        {
            parameters = serviceEntry.ConvertParameters(parameters);

            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(serviceEntry.FallbackMethodExecutor, nameof(serviceEntry.FallbackMethodExecutor));
            Check.NotNull(serviceEntry.FallbackProvider, nameof(serviceEntry.FallbackProvider));
            object instance = null;
            var fallbackServiceKey = RpcContext.Context.GetFallbackServiceKey();
            instance = fallbackServiceKey.IsNullOrEmpty()
                ? EngineContext.Current.Resolve(serviceEntry.FallbackProvider.Type)
                : EngineContext.Current.ResolveNamed(fallbackServiceKey, serviceEntry.FallbackProvider.Type);
            if (instance == null)
            {
                throw new NotFindFallbackInstanceException(
                    $"The implementation class of the failed callback was not found;{Environment.NewLine}" +
                    $"Type:{serviceEntry.FallbackProvider.Type.FullName},fallbackServiceKey:{fallbackServiceKey}");
            }

            object result = null;
            try
            {
                if (serviceEntry.FallbackMethodExecutor.IsMethodAsync)
                {
                    result = await serviceEntry.FallbackMethodExecutor.ExecuteAsync(instance, parameters);
                }
                else
                {
                    result = serviceEntry.FallbackMethodExecutor.Execute(instance, parameters);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new FallbackException(ex.Message, ex.InnerException);
            }

            return result;
        }
    }
}