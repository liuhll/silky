using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    internal class DefaultAppointAddressInvoker : IAppointAddressInvoker
    {
        private readonly ILocalExecutor _localExecutor;
        private readonly IRemoteExecutor _remoteExecutor;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultAppointAddressInvoker(ILocalExecutor localExecutor,
            IRemoteExecutor remoteExecutor,
            IServiceEntryLocator serviceEntryLocator,
            IHttpContextAccessor httpContextAccessor)
        {
            _localExecutor = localExecutor;
            _remoteExecutor = remoteExecutor;
            _serviceEntryLocator = serviceEntryLocator;
            _httpContextAccessor = httpContextAccessor;
        }


        public Task<object> Invoke([NotNull] string address, [NotNull] ServiceEntry serviceEntry, object[] parameters,
            string serviceKey = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.SetHttpMessageId();
                httpContext.SetHttpHandleAddressInfo();
            }

            Check.NotNull(address, nameof(address));
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            if (SilkyEndpointHelper.IsLocalRpcAddress(address))
            {
                return _localExecutor.Execute(serviceEntry, parameters, serviceKey);
            }

            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.SelectedServerEndpoint, address);
            return _remoteExecutor.Execute(serviceEntry, parameters, serviceKey);
        }

        public async Task<T> Invoke<T>([NotNull] string address, [NotNull] ServiceEntry serviceEntry,
            object[] parameters,
            string serviceKey = null)
        {
            var result = await Invoke(address, serviceEntry, parameters, serviceKey);
            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }

        public Task<object> Invoke([NotNull] string address, [NotNull] string serviceEntryId, object[] parameters,
            string serviceKey = null)
        {
            Check.NotNull(address, nameof(address));
            Check.NotNull(serviceEntryId, nameof(serviceEntryId));
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            if (serviceEntry == null)
            {
                throw new NotFindServiceEntryException(
                    $"There is no service entry with id {serviceEntryId} in the system");
            }

            return Invoke(address, serviceEntry, parameters, serviceKey);
        }

        public async Task<T> Invoke<T>([NotNull] string address, [NotNull] string serviceEntryId, object[] parameters,
            string serviceKey = null)
        {
            var result = await Invoke(address, serviceEntryId, parameters, serviceKey);
            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }
    }
}