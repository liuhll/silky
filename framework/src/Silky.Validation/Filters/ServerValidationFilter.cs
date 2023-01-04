using System;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Filters;

namespace Silky.Validation.Filters;

public class ServerValidationFilter : IServerFilter
{
    
    private readonly IMethodInvocationValidator _methodInvocationValidator;
    private AppSettingsOptions _appSettingsOptions;

    public ServerValidationFilter(IMethodInvocationValidator methodInvocationValidator,
        IOptionsMonitor<AppSettingsOptions> appSettingsOptions)
    {
        _methodInvocationValidator = methodInvocationValidator;
        _appSettingsOptions = appSettingsOptions.CurrentValue;
        appSettingsOptions.OnChange((options, s) => _appSettingsOptions = options);
    }

    public void OnActionExecuting(ServerExecutingContext context)
    {
        if (!_appSettingsOptions.AutoValidationParameters) return;
        if (RpcContext.Context.GetInvokeAttachment(AttachmentKeys.ValidationParametersInClient)?.ConventTo<bool>() ==
            true) return;
        _methodInvocationValidator.Validate(
            new MethodInvocationValidationContext(context.ServiceEntry.MethodInfo, context.Parameters));
    }

    public void OnActionExecuted(ServerExecutedContext context)
    {
    }
    
}