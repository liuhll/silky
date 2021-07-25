using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Tcc
{
    public static class ParticipantExtensions
    {
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
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                Debug.Assert(serviceEntry != null, "invocation cannot be empty");
                var context = RpcContext.GetContext().GetTransactionContext();
                await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
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
                var context = RpcContext.GetContext().GetTransactionContext();
                await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
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