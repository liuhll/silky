using Microsoft.Extensions.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Auditing;
using Silky.Rpc.Configuration;
using Silky.Rpc.Transport.Codec;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultMessageCodec(this IServiceCollection services)
        {
            if (!services.IsAdded(typeof(ITransportMessageDecoder)))
            {
                services.AddTransient<ITransportMessageDecoder, DefaultTransportMessageDecoder>();
            }

            if (!services.IsAdded(typeof(ITransportMessageEncoder)))
            {
                services.AddTransient<ITransportMessageEncoder, DefaultTransportMessageEncoder>();
            }

            return services;
        }
        
         public static IServiceCollection AddAuditing(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddOptions<AuditingOptions>()
                    .Bind(configuration.GetSection(AuditingOptions.Auditing));
                services.AddTransient<IAuditSerializer, JsonNetAuditSerializer>();
                return services;
            }
    }
}