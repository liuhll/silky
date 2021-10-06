using System;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Validation.Filters
{
    public class ValidationFilter : IClientFilter, IScopedDependency
    {
        public int Order { get; } = Int32.MaxValue;

        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private AppSettingsOptions _appSettingsOptions;

        public ValidationFilter(IMethodInvocationValidator methodInvocationValidator,
            IOptionsMonitor<AppSettingsOptions> appSettingsOptions,
            IServiceEntryLocator serviceEntryLocator)
        {
            _methodInvocationValidator = methodInvocationValidator;
            _serviceEntryLocator = serviceEntryLocator;
            _appSettingsOptions = appSettingsOptions.CurrentValue;
            appSettingsOptions.OnChange((options, s) => _appSettingsOptions = options);
        }

        public void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage)
        {
            if (!_appSettingsOptions.AutoValidationParameters) return;
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(serviceEntry.MethodInfo, remoteInvokeMessage.Parameters));
        }

        public void OnActionExecuted(RemoteResultMessage context)
        {
        }
    }
}