using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Validation.Filters
{
    public class ClientValidationFilter : IAsyncClientFilter
    {
        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public ClientValidationFilter(IMethodInvocationValidator methodInvocationValidator,
            IServiceEntryLocator serviceEntryLocator)
        {
            _methodInvocationValidator = methodInvocationValidator;
            _serviceEntryLocator = serviceEntryLocator;
        }


        public async Task OnActionExecutionAsync(ClientInvokeExecutingContext context,
            ClientInvokeExecutionDelegate next)
        {
            var remoteInvokeMessage = context.RemoteInvokeMessage;
            if (!EngineContext.Current.ApplicationOptions.AutoValidationParameters) return;
            if (remoteInvokeMessage.ParameterType != ParameterType.Rpc) return;
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
            if (serviceEntry == null) return;
            if (serviceEntry.IsLocal) return;
            await _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(serviceEntry.MethodInfo, remoteInvokeMessage.Parameters));
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ValidationParametersInClient, true);
            await next();
        }
    }
}