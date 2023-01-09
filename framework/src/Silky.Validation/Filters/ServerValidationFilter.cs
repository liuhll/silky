using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Filters;

namespace Silky.Validation.Filters;

public class ServerValidationFilter : IServerFilter
{
    private readonly IMethodInvocationValidator _methodInvocationValidator;

    public ServerValidationFilter(IMethodInvocationValidator methodInvocationValidator)
    {
        _methodInvocationValidator = methodInvocationValidator;
    }

    public void OnActionExecuting(ServerInvokeExecutingContext context)
    {
        if (!EngineContext.Current.ApplicationOptions.AutoValidationParameters) return;
        if (RpcContext.Context.GetInvokeAttachment(AttachmentKeys.ValidationParametersInClient)?.ConventTo<bool>() ==
            true) return;
        _methodInvocationValidator.Validate(
            new MethodInvocationValidationContext(context.ServiceEntry.MethodInfo, context.Parameters));
    }

    public void OnActionExecuted(ServerInvokeExecutedContext context)
    {
    }
}