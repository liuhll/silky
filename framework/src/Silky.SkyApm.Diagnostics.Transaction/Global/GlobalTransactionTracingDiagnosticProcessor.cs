using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Diagnostics;
using SkyApm;
using SkyApm.Diagnostics;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Transaction.Global
{
    public class GlobalTransactionTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = TransactionDiagnosticListenerNames.DiagnosticGlobalTransactionListener;
        private readonly ISilkySegmentContextFactory _segmentContextFactory;

        public GlobalTransactionTracingDiagnosticProcessor(ISilkySegmentContextFactory segmentContextFactory)
        {
            _segmentContextFactory = segmentContextFactory;
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalPreTryHandle)]
        public void GlobalPreTryHandle([Object] GlobalTransactionEventData eventData)
        {
            var entryContext = _segmentContextFactory.GetEntryContext(eventData.ServiceEntryId);
            entryContext.Span.AddTag("TransactionType", eventData.Type.ToString());
            // entryContext.Span.AddTag("TransactionRole",eventData.Role.ToString());

            var localContext =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Trying));
            localContext.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-PreTry"),
                LogEvent.Message($"Start a {eventData.Type} transaction"));
            localContext.Span.AddTag("TransId", eventData.Transaction.TransId);
            localContext.Span.AddTag("TransType", eventData.Transaction.TransType.ToString());
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalTryingHandle)]
        public void GlobalTryingHandle([Object] GlobalTransactionEventData eventData)
        {
            var context = _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Trying));
            context.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-Trying"));
            _segmentContextFactory.ReleaseContext(context);
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalConfirmingHandle)]
        public void GlobalConfirmingHandle([Object] GlobalTransactionEventData eventData)
        {
            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Confirming));
            context.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-Confirming"));
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalConfirmedHandle)]
        public void GlobalConfirmedHandle([Object] GlobalTransactionEventData eventData)
        {
            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Confirming));
            context.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-Confirmed"));
            _segmentContextFactory.ReleaseContext(context);
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalCancelingHandle)]
        public void GlobalCancelingHandle([Object] GlobalTransactionEventData eventData)
        {
            var tryingContext =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Trying));
            _segmentContextFactory.ReleaseContext(tryingContext);

            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Canceling));
            context.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-Canceling"));
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.GlobalCanceledHandle)]
        public void GlobalCanceledHandle([Object] GlobalTransactionEventData eventData)
        {
            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData, ActionStage.Canceling));
            context.Span.AddLog(LogEvent.Event($"{eventData.Type}-{eventData.Role}-Canceled"));
            _segmentContextFactory.ReleaseContext(context);
        }

        private string GetOperationName(GlobalTransactionEventData eventData, ActionStage actionStage)
        {
            return $"{eventData.Type}.Global.{actionStage}";
        }
    }
}