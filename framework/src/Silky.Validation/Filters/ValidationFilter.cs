using System;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Rpc.Runtime.Filters;

namespace Silky.Validation.Filters
{
    public class ValidationFilter : IClientFilter
    {
        public int Order { get; } = Int32.MaxValue;

        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private readonly AppSettingsOptions _appSettingsOptions;

        public ValidationFilter(IMethodInvocationValidator methodInvocationValidator,
            IOptions<AppSettingsOptions> appSettingsOptions)
        {
            _methodInvocationValidator = methodInvocationValidator;
            _appSettingsOptions = appSettingsOptions.Value;
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