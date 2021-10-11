using Silky.Core.Serialization;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using Silky.Transaction.Abstraction.Diagnostics;
using SkyApm;
using SkyApm.Diagnostics;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Transaction.Participant
{
    public class ParticipantTransactionTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } =
            TransactionDiagnosticListenerNames.DiagnosticParticipantTransactionListener;

        private readonly ISilkySegmentContextFactory _segmentContextFactory;
        private readonly ISerializer _serializer;

        public ParticipantTransactionTracingDiagnosticProcessor(ISilkySegmentContextFactory segmentContextFactory,
            ISerializer serializer)
        {
            _segmentContextFactory = segmentContextFactory;
            _serializer = serializer;
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.ParticipantBeginHandle)]
        public void TccParticipantBeginHandle([Object] ParticipantTransactionEventData eventData)
        {
            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData));
            context.Span.AddLog(LogEvent.Event($"Tcc Participant Transaction Action {eventData.Context.Action}Begin"),
                LogEvent.Message($"--> transactionContext:{_serializer.Serialize(eventData.Context)}"));
            context.Span.AddTag("TransId", eventData.Context.TransId);
            context.Span.AddTag("ParticipantId", eventData.Context.ParticipantId);
            context.Span.AddTag("TransactionRole", eventData.Role.ToString());
            context.Span.AddTag("Action", eventData.Context.Action.ToString());
        }

        [DiagnosticName(TransactionDiagnosticListenerNames.ParticipantEndHandle)]
        public void TccParticipantEndHandle([Object] ParticipantTransactionEventData eventData)
        {
            var context =
                _segmentContextFactory.GetTransactionContext(GetOperationName(eventData));
            context.Span.AddLog(LogEvent.Event($"Tcc Participant Transaction Action {eventData.Context.Action}End"));
            _segmentContextFactory.ReleaseContext(context);
        }

        private string GetOperationName(ParticipantTransactionEventData eventData)
        {
            return $"{eventData.Type}.{eventData.Role}.{eventData.Context.Action}";
        }
    }
}