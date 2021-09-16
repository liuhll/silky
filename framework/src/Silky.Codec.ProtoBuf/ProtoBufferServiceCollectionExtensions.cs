using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Codec;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Codec;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProtoBufferServiceCollectionExtensions
    {
        public static IServiceCollection AddProtoBufferCodec(this IServiceCollection services)
        {
            if (!services.IsAdded(typeof(ITransportMessageDecoder)))
            {
                services.AddScoped<ITransportMessageDecoder, ProtoBufferTransportMessageDecoder>();
            }
            else
            {
                services.Replace(
                    ServiceDescriptor.Scoped<ITransportMessageDecoder, ProtoBufferTransportMessageDecoder>());
            }

            if (!services.IsAdded(typeof(ITransportMessageEncoder)))
            {
                services.AddScoped<ITransportMessageEncoder, ProtoBufferTransportMessageEncoder>();
            }
            else
            {
                services.Replace(
                    ServiceDescriptor.Scoped<ITransportMessageEncoder, ProtoBufferTransportMessageEncoder>());
            }

            return services;
        }
    }
}