using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Rpc.Transport;
using Silky.SkyApm.Abstraction.Collections;
using Silky.Transaction.Abstraction;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Rpc.Factory
{
    public class SilkyRpcSegmentContextFactory : ISilkyRpcSegmentContextFactory
    {
        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;

        public SilkyRpcSegmentContextFactory(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor)
        {
            _tracingContext = tracingContext;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
        }

        public SegmentContext GetEntryContext(string serviceEntryId)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null)
            {
                var carrierHeader = new SilkyCarrierHeaderCollection(RpcContext.Context);
                var serverAddress = RpcContext.Context.GetServerAddress();
                context =
                    _tracingContext.CreateEntrySegmentContext($"[ServerHandle]{serviceEntryId}",
                        carrierHeader);
                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = SilkyComponents.SilkyRpc;
                context.Span.Peer = serverAddress;
            }

            return context;
        }

        public SegmentContext GetExitSContext(string serviceEntryId)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null)
            {
                var serverAddress = RpcContext.Context.GetServerAddress();
                context = _tracingContext.CreateExitSegmentContext($"[ClientInvoke]{serviceEntryId}",
                    serverAddress, new SilkyCarrierHeaderCollection(RpcContext.Context));

                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = SilkyComponents.SilkyRpc;
            }

            return context;
        }

        public SegmentContext GetTransContext(TransactionRole role, ActionStage action)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null)
            {
                var operationName = GetOperationName(role, action);
                context = _tracingContext.CreateLocalSegmentContext(operationName);
                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = GetComponent(role);
            }

            return context;
        }

        public void ReleaseContext(SegmentContext context)
        {
            _tracingContext.Release(context);
        }

        private StringOrIntValue GetComponent(TransactionRole role)
        {
            StringOrIntValue componentName;
            switch (role)
            {
                case TransactionRole.Start:
                    componentName = SilkyComponents.SilkyStartTransaction;
                    break;
                case TransactionRole.Participant:
                    componentName = SilkyComponents.SilkyParticipantTransaction;
                    break;
                default:
                    throw new SilkyException("There is no such distributed transaction role");
            }

            return componentName;
        }


        private string GetOperationName(TransactionRole transactionRole, ActionStage actionStage)
        {
            var operationName = string.Empty;
            switch (transactionRole)
            {
                case TransactionRole.Start:
                    operationName = $"Start.{actionStage}";
                    break;
                case TransactionRole.Participant:
                    operationName = $"Participant.{actionStage}";
                    break;
                default:
                    throw new SilkyException("There is no such distributed transaction role");
            }

            return operationName;
        }
    }
}