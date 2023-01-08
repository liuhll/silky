using System.Linq;
using System.Threading.Tasks;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Utils;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Exceptions;

namespace Silky.Transaction.Filters
{
    public class AsyncTransactionFilter : IAsyncClientFilter
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IServerManager _serverManager;

        public AsyncTransactionFilter(IServiceEntryLocator serviceEntryLocator,
            IServerManager serverManager)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _serverManager = serverManager;
        }

        public async Task OnActionExecutionAsync(ClientInvokeExecutingContext invokeContext,
            ClientInvokeExecutionDelegate next)
        {
            var context = SilkyTransactionContextHolder.Instance.Get();
            if (context == null)
            {
                await next();
            }
            else
            {
                var remoteInvokeMessage = invokeContext.RemoteInvokeMessage;
                var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteInvokeMessage.ServiceEntryId);
                if (serviceEntry == null)
                {
                    var serviceEntryDescriptor =
                        _serverManager.GetServiceEntryDescriptor(remoteInvokeMessage.ServiceEntryId);
                    if (serviceEntryDescriptor.IsDistributeTransaction && !RpcContext.Context.IsGateway())
                    {
                        throw new TransactionException(
                            "Distributed transaction service does not support invoke through templateService, please call the service through interface proxy.");
                    }

                    await next();
                }
                else
                {
                    if (!serviceEntry.IsTransactionServiceEntry())
                    {
                        await next();
                    }
                    else
                    {
                        var participantId = context.ParticipantId;
                        var participant = BuildParticipant(context, remoteInvokeMessage.ServiceEntryId,
                            RpcContext.Context.GetServiceKey(),
                            remoteInvokeMessage.Parameters);

                        if (participant != null)
                        {
                            var rpcTransactionContext = new TransactionContext()
                            {
                                Action = context.Action,
                                ParticipantId = participant.ParticipantId,
                                ParticipantRefId = context.ParticipantRefId,
                                TransactionRole = context.TransactionRole,
                                TransId = context.TransId,
                                TransType = context.TransType,
                            };
                            if (context.TransactionRole == TransactionRole.Participant)
                            {
                                rpcTransactionContext.ParticipantRefId = context.ParticipantId;
                            }

                            RpcContext.Context.SetTransactionContext(rpcTransactionContext);
                        }

                        await next();

                        if (context.TransactionRole == TransactionRole.Participant)
                        {
                            SilkyTransactionHolder.Instance.RegisterParticipantByNested(participantId, participant);
                        }
                        else
                        {
                            SilkyTransactionHolder.Instance.RegisterStarterParticipant(participant);
                        }
                    }
                }
            }
        }
        
        private IParticipant? BuildParticipant(TransactionContext context, string serviceEntryId, string serviceKey,
            object[] parameters)
        {
            if (context.Action != ActionStage.Trying)
            {
                return null;
            }

            return new SilkyParticipant()
            {
                ParticipantId = GuidGenerator.CreateGuidStrWithNoUnderline(),
                TransId = context.TransId,
                TransType = context.TransType,
                ServiceEntryId = serviceEntryId,
                ServiceKey = serviceKey,
                Parameters = parameters,
                InvokeAttachments = RpcContext.Context.GetInvokeAttachments().ToDictionary(p => p.Key, p => p.Value),
                Role = TransactionRole.Participant
                // Status = context.Action
            };
        }
    }
}