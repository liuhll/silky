using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
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
            switch (context.Action)
            {
                case ActionStage.Trying:
                    IParticipant participant = null;
                    try
                    {
                        participant = await _executor.PreTryParticipant(context, invocation);
                        if (participant == null)
                        {
                            await invocation.ProceedAsync();
                        }
                        else
                        {
                            context.TransactionRole = participant.Role;
                            SilkyTransactionContextHolder.Instance.Set(context);
                            await invocation.ProceedAsync();
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