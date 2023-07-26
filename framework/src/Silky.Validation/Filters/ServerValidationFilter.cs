using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Filters;

namespace Silky.Validation.Filters;

public class ServerValidationFilter : IAsyncServerFilter
{
    private readonly IMethodInvocationValidator _methodInvocationValidator;

    public ServerValidationFilter(IMethodInvocationValidator methodInvocationValidator)
    {
        _methodInvocationValidator = methodInvocationValidator;
    }
    
    public async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
    {
        if (!EngineContext.Current.ApplicationOptions.AutoValidationParameters) return;
        if (RpcContext.Context.GetInvokeAttachment(AttachmentKeys.ValidationParametersInClient)?.ConventTo<bool>() ==
            true) return;
        await _methodInvocationValidator.Validate(
            new MethodInvocationValidationContext(context.ServiceEntry.MethodInfo, context.Parameters));
        await next();
    }
}