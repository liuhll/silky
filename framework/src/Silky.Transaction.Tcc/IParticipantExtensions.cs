using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
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
                MethodType methodType)

            {
                if (localInvocation != null)
                {
                    await localInvocation.ExcuteTccMethod(methodType, RpcContext.GetContext().GetTransactionContext());
                }
                else if (participant.Invocation != null)
                {
                    await participant.Invocation.ExcuteTccMethod(methodType,
                        RpcContext.GetContext().GetTransactionContext());
                }
                else
                {
                    var excutorInfo = serviceEntry.GetTccExcutorInfo(participant.ServiceKey, methodType);
                    if (excutorInfo.Item2)
                    {
                        await excutorInfo.Item1.ExecuteAsync(excutorInfo.Item3, participant.Parameters);
                    }
                    else
                    {
                        excutorInfo.Item1.Execute(excutorInfo.Item3, participant.Parameters);
                    }
                }
            }

            if (serviceEntry.IsLocal)
            {
                participant.Status = stage;
                await TransRepositoryStore.UpdateParticipantStatus(participant);

                if (stage == ActionStage.Confirming)
                {
                    await LocalExecutor(invocation, participant, MethodType.Confirm);
                }
                else
                {
                    await LocalExecutor(invocation, participant, MethodType.Cancel);
                }

                await TransRepositoryStore.RemoveParticipant(participant);
            }
            else
            {
                RpcContext.GetContext().SetTransactionContext(SilkyTransactionContextHolder.Instance.Get());
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
                TransactionRole = participant.Role,
                TransType = TransactionType.Tcc
            };

            SilkyTransactionContextHolder.Instance.Set(context);
        }
    }
}