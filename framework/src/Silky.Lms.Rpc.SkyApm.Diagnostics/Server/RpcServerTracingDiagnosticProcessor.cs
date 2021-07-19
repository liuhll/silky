using System.Collections.Concurrent;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Diagnostics;
using Silky.Lms.Rpc.Utils;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.Lms.Rpc.SkyApm.Diagnostics
{
    public class RpcServerTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticServerListenerName;

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;

        private readonly ISerializer _serializer;
        // private readonly ConcurrentDictionary<string, SegmentContext> _contexts = new();

        public RpcServerTracingDiagnosticProcessor(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IConfigAccessor configAccessor,
            ISerializer serializer)
        {
            _tracingContext = tracingContext;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
        }


        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcServerHandler)]
        public void BeginRpcServerHandle([Object] RpcInvokeEventData eventData)
        {
            var carrierHeader = new RpcCarrierHeaderCollection(eventData.Message);
            var context = _tracingContext.CreateEntrySegmentContext(eventData.Operation, carrierHeader);

            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.LmsRpc;
            context.Span.Peer = new StringOrIntValue(eventData.RemoteAddress);
            context.Span.AddLog(LogEvent.Event("Rpc Server Handle Begin"),
                LogEvent.Message($"Request starting {eventData.Operation}"));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.AddTag(Tags.RPC_SERVICEID, eventData.Operation.ToString());
            context.Span.AddTag(Tags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));

            //   _contexts[eventData.MessageId + eventData.ServiceId] = context;
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcServerHandler)]
        public void EndRpcServerHandle([Object] RpcResultEventData eventData)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null) return;
            context.Span.AddLog(LogEvent.Event("Rpc Server Handle End"));
            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcServerHandler)]
        public void ErrorRpcServerHandle([Object] RpcResultEventData eventData)
        {
        }
    }
}