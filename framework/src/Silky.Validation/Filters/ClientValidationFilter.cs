using System;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Validation.Filters
{
    public class ClientValidationFilter : IClientFilter, IScopedDependency
    {
        public int Order { get; } = Int32.MaxValue;

        private readonly IMethodInvocationValidator _methodInvocationValidator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private AppSettingsOptions _appSettingsOptions;

        public ClientValidationFilter(IMethodInvocationValidator methodInvocationValidator,
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
            if (remoteInvokeMessage.ParameterType != ParameterType.Rpc) return;
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
            if (serviceEntry == null) return;
            _methodInvocationValidator.Validate(
                new MethodInvocationValidationContext(serviceEntry.MethodInfo, remoteInvokeMessage.Parameters));
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ValidationParametersInClient, true);
        }

        public void OnActionExecuted(RemoteResultMessage context)
        {
        }
    }
}