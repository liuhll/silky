using System;
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

    public static Task SendForSilky<T>(
        this ISendEndpoint endpoint,
        T message,
        Action<SendContext<T>> callback,
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

            callback.Invoke(context);
        }, cancellationToken);
    }

    public static Task SendForSilky<T>(
        this ISendEndpoint endpoint,
        T message,
        Func<SendContext<T>, Task> callback,
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

            return callback.Invoke(context);
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

    public static Task SendForSilky(
        this ISendEndpoint endpoint,
        object message,
        Action<SendContext<object>> callback,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.Send(message, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }

            callback.Invoke(context);
        }, cancellationToken);
    }

    public static Task SendForSilky(
        this ISendEndpoint endpoint,
        object message,
        Func<SendContext<object>, Task> callback,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.Send(message, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }

            return callback.Invoke(context);
        }, cancellationToken);
    }

    public static Task SendBatchForSilky<T>(
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

    public static Task SendBatchForSilky<T>(
        this ISendEndpoint endpoint,
        IEnumerable<T> messages,
        Action<SendContext<object>> callback,
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

            callback.Invoke(context);
        }, cancellationToken);
    }

    public static Task SendBatchForSilky<T>(
        this ISendEndpoint endpoint,
        IEnumerable<T> messages,
        Func<SendContext<T>, Task> callback,
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

            return callback.Invoke(context);
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

    public static Task SendBatchForSilky(
        this ISendEndpoint endpoint,
        IEnumerable<object> messages,
        Action<SendContext<object>> callback,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.SendBatch(messages, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }

            callback.Invoke(context);
        }, cancellationToken);
    }

    public static Task SendBatchForSilky(
        this ISendEndpoint endpoint,
        IEnumerable<object> messages,
        Func<SendContext<object>, Task> callback,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        RpcContext.Context.SetMqInvokeAddressInfo();
        return endpoint.SendBatch(messages, context =>
        {
            foreach (var header in RpcContext.Context.GetInvokeAttachments())
            {
                context.Headers.Set(header.Key, header.Value);
            }

            return callback.Invoke(context);
        }, cancellationToken);
    }
}