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

        public static async Task ParticipantConfirm(this IParticipant participant)
        {
            SetContext(ActionStage.Confirming, participant);
            var invocation = participant.Invocation;
            if (invocation != null)
            {
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                Debug.Assert(serviceEntry != null, "invocation cannot be empty");
                var context = RpcContext.GetContext().GetTransactionContext();
                await invocation.ExcuteTccMethod(TccMethodType.Confirm, context);
            }
            else
            {
                RpcContext.GetContext().SetTransactionContext(SilkyTransactionContextHolder.Get());
                var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
                var remoteServiceEntry = serviceEntryLocator.GetServiceEntryById(participant.ServiceId);
                var serviceExecutor = EngineContext.Current.Resolve<IServiceExecutor>();
                await serviceExecutor.Execute(remoteServiceEntry, participant.Parameters, participant.ServiceKey);
            }
        }

        public static async Task ParticipantConfirm(this IParticipant participant, ISilkyMethodInvocation invocation)
        {
            SetContext(ActionStage.Confirming, participant);
            var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            var serviceEntry = serviceEntryLocator.GetServiceEntryById(participant.ServiceId);
            if (serviceEntry.IsLocal)
            {
                var context = RpcContext.GetContext().GetTransactionContext();
                await invocation.ExcuteTccMethod(TccMethodType.Confirm, context);
            }
            else
            {
                RpcContext.GetContext().SetTransactionContext(SilkyTransactionContextHolder.Get());
                var serviceExecutor = EngineContext.Current.Resolve<IServiceExecutor>();
                await serviceExecutor.Execute(serviceEntry, participant.Parameters, participant.ServiceKey);
            }
        }


        public static async Task ParticipantCancel(this IParticipant participant)
        {
            SetContext(ActionStage.Canceling, participant);
            var invocation = participant.Invocation;
            if (invocation != null && participant.Role == TransactionRole.Start)
            {
                try
                {
                    var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                    Debug.Assert(serviceEntry != null, "invocation cannot be empty");
                    var context = RpcContext.GetContext().GetTransactionContext();
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
                    participant.Status = ActionStage.Canceled;
                    await TransRepositoryStore.UpdateParticipantStatus(participant);
                }
                catch (Exception e)
                {
                    Logger.LogError("Participant cancel exception", e.Message);
                }
            }
            else
            {
                RpcContext.GetContext().SetTransactionContext(SilkyTransactionContextHolder.Get());
                var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
                var remoteServiceEntry = serviceEntryLocator.GetServiceEntryById(participant.ServiceId);
                var serviceExecutor = EngineContext.Current.Resolve<IServiceExecutor>();
                await serviceExecutor.Execute(remoteServiceEntry, participant.Parameters, participant.ServiceKey);
            }
        }


        public static async Task ParticipantCancel(this IParticipant participant, ISilkyMethodInvocation invocation)
        {
            SetContext(ActionStage.Canceling, participant);
            var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            var serviceEntry = serviceEntryLocator.GetServiceEntryById(participant.ServiceId);
            if (serviceEntry.IsLocal)
            {
                try
                {
                    var context = RpcContext.GetContext().GetTransactionContext();
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
                    participant.Status = ActionStage.Canceled;
                    await TransRepositoryStore.UpdateParticipantStatus(participant);
                }
                catch (Exception e)
                {
                    Logger.LogError("Participant cancel exception", e.Message);
                }
               
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
                TransactionRole = TransactionRole.Start,
                TransType = TransactionType.Tcc
            };

            SilkyTransactionContextHolder.Set(context);
            RpcContext.GetContext().SetTransactionContext(context);
        }
    }
}