using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Codec;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultMessageCodec(this IServiceCollection services)
        {
            if (!services.IsAdded(typeof(ITransportMessageDecoder)))
            {
                services.AddScoped<ITransportMessageDecoder, DefaultTransportMessageDecoder>();
            }

            if (!services.IsAdded(typeof(ITransportMessageEncoder)))
            {
                services.AddScoped<ITransportMessageEncoder, DefaultTransportMessageEncoder>();
            }

            return services;
        }

        public static IServiceCollection AddDefaultServiceGovernancePolicy(this IServiceCollection services)
        {
            if (!services.IsAdded(typeof(IInvokePolicyBuilder)))
            {
                services.AddScoped<IInvokePolicyBuilder, DefaultInvokePolicyBuilder>();
            }

            return services;
        }
    }
}