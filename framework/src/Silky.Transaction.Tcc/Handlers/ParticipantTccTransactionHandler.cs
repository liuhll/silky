using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.DynamicProxy;
using Silky.Core.Logging;
using Silky.Rpc.Extensions;
using Silky.Transaction.Cache;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Diagnostics;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private readonly TccTransactionExecutor _executor = TccTransactionExecutor.Executor;

        private static readonly DiagnosticListener s_diagnosticListener =
            new(TransactionDiagnosticListenerNames.DiagnosticParticipantTransactionListener);

        public ILogger<ParticipantTccTransactionHandler> Logger { get; set; }

        public ParticipantTccTransactionHandler()
        {
            Logger = NullLogger<ParticipantTccTransactionHandler>.Instance;
        }

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            WriteTracing(TransactionDiagnosticListenerNames.ParticipantBeginHandle, context);
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
                    catch (Exception ex)
                    {
                        if (participant != null)
                        {
                            ParticipantCacheManager.Instance.RemoveByKey(participant.ParticipantId);
                        }

                        await TransRepositoryStore.RemoveParticipant(participant);
                        Logger.LogException(ex);
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

            WriteTracing(TransactionDiagnosticListenerNames.ParticipantEndHandle, context);
        }

        private void WriteTracing(string tracingName, TransactionContext context)
        {
            if (s_diagnosticListener.IsEnabled(tracingName))
            {
                s_diagnosticListener.Write(tracingName, new ParticipantTransactionEventData()
                {
                    Context = context,
                    Role = TransactionRole.Participant,
                    Type = TransactionType.Tcc
                });
            }
        }
    }
}