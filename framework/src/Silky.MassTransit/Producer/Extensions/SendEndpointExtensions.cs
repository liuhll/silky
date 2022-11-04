using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Silky.Core.Runtime.Rpc;
using Silky.MassTransit.Extensions;

namespace MassTransit;

public static class SendEndpointExtensions
{
    public static Task SendForSilky<T>(
        this ISendEndpoint endpoint,
        T message,
        CancellationToken cancellationToken = default(CancellationToken))
        where T : class
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.Send(message, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }
        }, cancellationToken);
    }

    public static Task SendForSilky(
        this ISendEndpoint endpoint,
        object message,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.Send(message, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }
        }, cancellationToken);
    }

    public static Task SendForSilky<T>(
        this ISendEndpoint endpoint,
        IEnumerable<T> messages,
        CancellationToken cancellationToken = default(CancellationToken))
        where T : class
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.SendBatch(messages, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }
        }, cancellationToken);
    }

    public static Task SendBatchForSilky(
        this ISendEndpoint endpoint,
        IEnumerable<object> messages,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.SendBatch(messages, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }
        }, cancellationToken);
    }
}