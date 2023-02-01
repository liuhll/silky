using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client;

public class NonSilkyExceptionFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
{
    private readonly ILogger<NonSilkyExceptionFailoverPolicyProvider> _logger;
    private readonly IServerManager _serverManager;

    public NonSilkyExceptionFailoverPolicyProvider(ILogger<NonSilkyExceptionFailoverPolicyProvider> logger, IServerManager serverManager)
    {
        _logger = logger;
        _serverManager = serverManager;
    }

    public override IAsyncPolicy<object> Create(string serviceEntryId)
    {
        IAsyncPolicy<object> policy = null;
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);

        if (serviceEntryDescriptor?.GovernanceOptions.RetryTimes > 0)
        {
            policy = Policy<object>
                .Handle<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.NonSilkyException)
                .WaitAndRetryAsync(serviceEntryDescriptor.GovernanceOptions.RetryTimes,
                    retryAttempt =>
                        TimeSpan.FromMilliseconds(serviceEntryDescriptor.GovernanceOptions
                            .RetryIntervalMillSeconds),
                    (outcome, timeSpan, retryNumber, context)
                        => OnRetry(retryNumber, outcome, context));
        }

        return policy;
    }
    
    private async Task OnRetry(int retryNumber, DelegateResult<object> outcome, Context context)
    {
        var serviceAddressModel = GetSelectedServerEndpoint();
        _logger.LogWarning(
            $"A non-framework exception occurred," +
            $" and the rpc call is retryed for the ({retryNumber})th time.");
        serviceAddressModel.MakeFusing(1000);
        if (OnInvokeFailover != null)
        {
            await OnInvokeFailover.Invoke(outcome, retryNumber, context, serviceAddressModel,
                FailoverType.NonSilkyFrameworkException);
        }
    }
    
    public override event RpcInvokeFailoverHandle OnInvokeFailover;

    public override FailoverType FailoverType { get; } = FailoverType.NonSilkyFrameworkException;
}