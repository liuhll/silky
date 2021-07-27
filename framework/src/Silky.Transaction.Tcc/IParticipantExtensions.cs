using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Repository;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc
{
    public static class ParticipantExtensions
    {
        public static ILogger<TccTransactionExecutor> Logger =
            EngineContext.Current.Resolve<ILogger<TccTransactionExecutor>>();

        public static async Task Executor(this IParticipant participant, ActionStage stage,
            ISilkyMethodInvocation invocation = null)
        {
            SetContext(stage, participant);
            var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            var serviceEntry = serviceEntryLocator.GetServiceEntryById(participant.ServiceId);

            async Task LocalExecutor(ISilkyMethodInvocation localInvocation, IParticipant localParticipant,
                TccMethodType methodType)
            {
                if (localInvocation != null)
                {
                    await localInvocation.ExcuteTccMethod(methodType, RpcContext.GetContext().GetTransactionContext());
                }
                else
                {
                    Debug.Assert(participant.Invocation != null, "participant.Invocation is not null");
                    await localParticipant.Invocation.ExcuteTccMethod(methodType,
                        RpcContext.GetContext().GetTransactionContext());
                }
            }

            if (serviceEntry.IsLocal)
            {
                participant.Status = stage;
                await TransRepositoryStore.UpdateParticipantStatus(participant);
                if (stage == ActionStage.Confirming)
                {
                    await LocalExecutor(invocation, participant, TccMethodType.Confirm);
                    participant.Status = ActionStage.Confirmed;
                }
                else
                {
                    await LocalExecutor(invocation, participant, TccMethodType.Cancel);
                    participant.Status = ActionStage.Canceled;
                }

                await TransRepositoryStore.UpdateParticipantStatus(participant);
            }
            else
            {
                RpcContext.GetContext().SetTransactionContext(SilkyTransactionContextHolder.Get());
                var serviceExecutor = EngineContext.Current.Resolve<IServiceExecutor>();
                await serviceExecutor.Execute(serviceEntry, participant.Parameters, participant.ServiceKey);
            }
        }


        private static void SetContext(ActionStage action, IParticipant participant)
        {
            var context = new TransactionContext()
            {
                Action = action,
                TransId = participant.TransId,
                ParticipantId = participant.ParticipantId,
                TransactionRole = SilkyTransactionContextHolder.Get().TransactionRole,
                TransType = TransactionType.Tcc
            };

            SilkyTransactionContextHolder.Set(context);
        }
    }
}