using System;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;

namespace Silky.Validation.Filters
{
    public class ValidationFilter : IClientFilter
    {
        public int Order { get; } = Int32.MaxValue;

        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private AppSettingsOptions _appSettingsOptions;

        public ValidationFilter(IMethodInvocationValidator methodInvocationValidator,
            IOptionsMonitor<AppSettingsOptions> appSettingsOptions)
        {
            _methodInvocationValidator = methodInvocationValidator;
            _appSettingsOptions = appSettingsOptions.CurrentValue;
            appSettingsOptions.OnChange((options, s) => _appSettingsOptions = options);
        }

        public void OnActionExecuting(ServiceEntryExecutingContext context)
        {
            if (!_appSettingsOptions.AutoValidationParameters) return;
            var serviceEntry = context.ServiceEntry;
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(serviceEntry.MethodInfo, context.Parameters));
        }

        public void OnActionExecuted(ServiceEntryExecutedContext context)
        {
        }
    }
}