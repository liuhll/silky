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

            async Task<object> LocalExecutor(IParticipant localParticipant,
                MethodType methodType)
            {
                var localExecutor = EngineContext.Current.Resolve<ILocalExecutor>();
                return await localExecutor.Execute(serviceEntry, localParticipant.Parameters,
                    localParticipant.ServiceKey,
                    methodType);
            }

            if (serviceEntry.IsLocal)
            {
                participant.Status = stage;
                await TransRepositoryStore.UpdateParticipantStatus(participant);
                object execResult = null;
                if (stage == ActionStage.Confirming)
                {
                    execResult = await LocalExecutor(participant, MethodType.Confirm);
                }
                else
                {
                    execResult = await LocalExecutor(participant, MethodType.Cancel);
                }

                if (invocation != null)
                {
                    invocation.ReturnValue = execResult;
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