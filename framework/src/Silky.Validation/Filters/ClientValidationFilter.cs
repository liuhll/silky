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
            if (!EngineContext.Current.ApplicationOptions.AutoValidationParameters)
            {
                await next();
            }
            else
            {
                var remoteInvokeMessage = context.RemoteInvokeMessage;

                if (remoteInvokeMessage.ParameterType != ParameterType.Rpc)
                {
                    await next();
                }
                else
                {
                    var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
                    if (serviceEntry == null)
                    {
                        await next();
                    }
                    else if (serviceEntry.IsLocal)
                    {
                        await next();
                    }
                    else
                    {
                        await _methodInvocationValidator.Validate(
                            new MethodInvocationValidationContext(serviceEntry.MethodInfo, remoteInvokeMessage.Parameters));
                        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ValidationParametersInClient, true);
                        await next();
                    }
                }
            }

        }
    }
}