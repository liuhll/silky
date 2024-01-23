using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.DbContext;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.Runtime.Rpc;

namespace Silky.MassTransit.Consumer;

public abstract class SilkyConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        using var serviceScope = EngineContext.Current.ServiceProvider.CreateScope();
        var rpcContextAccessor = serviceScope.ServiceProvider.GetRequiredService<IRpcContextAccessor>();
        rpcContextAccessor.RpcContext = RpcContext.Context;
        rpcContextAccessor.RpcContext.RpcServices = serviceScope.ServiceProvider;
        foreach (var header in context.Headers)
        {
            rpcContextAccessor.RpcContext.SetInvokeAttachment(header.Key, header.Value);
        }

        var unitOfWorkAttribute =
            GetType().GetMethod("ConsumeWork", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>()
                .FirstOrDefault();
        var silkyDbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>();
        var isManualSaveChanges =
            GetType().GetMethod("ConsumeWork", BindingFlags.NonPublic | BindingFlags.Instance)?.CustomAttributes
                .OfType<ManualCommitAttribute>().Any();
        if (unitOfWorkAttribute != null)
        {
            silkyDbContextPool?.BeginTransaction(unitOfWorkAttribute.EnsureTransaction);
        }

        try
        {
            await ConsumeWork(context);
            if (unitOfWorkAttribute == null)
            {
                if (isManualSaveChanges == false)
                    await silkyDbContextPool?.SavePoolNowAsync();
            }
            else
            {
                silkyDbContextPool?.CommitTransaction();
            }
        }
        catch (Exception e)
        {
            if (unitOfWorkAttribute != null)
            {
                silkyDbContextPool?.RollbackTransaction();
            }

            throw;
        }
        finally
        {
            silkyDbContextPool?.CloseAll();
        }
    }

    protected abstract Task ConsumeWork(ConsumeContext<TMessage> context);
}