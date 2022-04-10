using Polly;
using Silky.Core;

namespace Silky.Rpc.Runtime.Client;

public static class AsyncPolicyExtensions
{
    public static IAsyncPolicy<object> WrapFallbackPolicy(this IAsyncPolicy<object> policy, string serviceEntryId,
        object[] parameters)
    {
        var fallbackPolicyProviders = EngineContext.Current.ResolveAll<IInvokeFallbackPolicyProvider>();

        foreach (var fallbackPolicyProvider in fallbackPolicyProviders)
        {
            var fallbackPolicy = fallbackPolicyProvider.Create(serviceEntryId, parameters);
            if (fallbackPolicy != null)
            {
                policy = policy.WrapAsync(fallbackPolicy);
            }
        }

        return policy;
    }
}