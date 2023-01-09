using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Configuration;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Validation.Filters
{
    public class ClientValidationFilter : IClientFilter
    {
        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public ClientValidationFilter(IMethodInvocationValidator methodInvocationValidator,
            IServiceEntryLocator serviceEntryLocator)
        {
            _methodInvocationValidator = methodInvocationValidator;
            _serviceEntryLocator = serviceEntryLocator;
        }
        
        public void OnActionExecuting(ClientInvokeExecutingContext context)
        {
            var remoteInvokeMessage = context.RemoteInvokeMessage;
            if (!EngineContext.Current.ApplicationOptions.AutoValidationParameters) return;
            if (remoteInvokeMessage.ParameterType != ParameterType.Rpc) return;
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
            if (serviceEntry == null) return;
            if (serviceEntry.IsLocal) return;
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(serviceEntry.MethodInfo, remoteInvokeMessage.Parameters));
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ValidationParametersInClient, true);
        }

        public void OnActionExecuted(ClientInvokeExecutedContext context)
        {
            
        }
    }
}