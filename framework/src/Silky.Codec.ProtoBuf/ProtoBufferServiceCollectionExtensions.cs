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
                services.AddTransient<ITransportMessageDecoder, ProtoBufferTransportMessageDecoder>();
            }
            else
            {
                services.Replace(ServiceDescriptor.Singleton<ITransportMessageDecoder, ProtoBufferTransportMessageDecoder>());
            }
            if (!services.IsAdded(typeof(ITransportMessageEncoder)))
            {
                services.AddTransient<ITransportMessageEncoder, ProtoBufferTransportMessageEncoder>();
            }
            else
            {
                services.Replace(ServiceDescriptor.Singleton<ITransportMessageEncoder, ProtoBufferTransportMessageEncoder>());
            }

            return services;
        }
    }
}