using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Cache;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private readonly TccTransactionExecutor _executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            IParticipant participant = null;
            switch (context.Action)
            {
                case ActionStage.Trying:
                    try
                    {
                        var preTryParticipantInfo = await _executor.PreTryParticipant(context, invocation);
                        participant = preTryParticipantInfo.Item1;
                        SilkyTransactionContextHolder.Instance.Set(preTryParticipantInfo.Item2);
                        await invocation.ProceedAsync();
                        if (participant != null)
                        {
                            participant.Status = ActionStage.Trying;
                            await TransRepositoryStore.UpdateParticipantStatus(participant);
                        }
                    }
                    catch (Exception e)
                    {
                        if (participant != null)
                        {
                            ParticipantCacheManager.Instance.RemoveByKey(participant.ParticipantId);
                        }
                        await TransRepositoryStore.RemoveParticipant(participant);
                        throw;
                    }
                    finally
                    {
                        SilkyTransactionContextHolder.Instance.Remove();
                    }

                    break;
                case ActionStage.Confirming:
                    var confirmingParticipantList = ParticipantCacheManager.Instance.Get(context.ParticipantId);
                    await _executor.ParticipantConfirm(invocation, confirmingParticipantList, context.ParticipantId);
                    break;

                case ActionStage.Canceling:
                    var cancelingParticipantList = ParticipantCacheManager.Instance.Get(context.ParticipantId);
                    await _executor.ParticipantCancel(invocation, cancelingParticipantList, context.ParticipantId);
                    break;
                default:
                    break;
            }
        }
    }
}