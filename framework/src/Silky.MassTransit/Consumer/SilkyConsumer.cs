using System.Threading.Tasks;
using MassTransit;
using Silky.Core.Runtime.Rpc;

namespace Silky.MassTransit.Consumer;

public abstract class SilkyConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    public Task Consume(ConsumeContext<TMessage> context)
    {
        foreach (var header in context.Headers)
        {
            RpcContext.Context.SetInvokeAttachment(header.Key, header.Value);
        }

        return ConsumeWork(context);
    }

    protected abstract Task ConsumeWork(ConsumeContext<TMessage> context);
}