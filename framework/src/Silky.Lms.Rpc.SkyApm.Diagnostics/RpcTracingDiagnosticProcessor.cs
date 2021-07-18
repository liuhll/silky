using System;
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
    public class RpcTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticListenerName;

        private readonly ConcurrentDictionary<string, SegmentContext> _contexts = new();

        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;

        public RpcTracingDiagnosticProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            IConfigAccessor configAccessor,
            ISerializer serializer)
        {
            _tracingContext = tracingContext;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
           
        }

        [DiagnosticName(RpcDiagnosticListenerNames.BeforeRpcInvoker)]
        public void RpcRequest([Object] RpcInvokeEventData eventData)
        {
            var host = NetUtil.GetRpcAddressModel().IPEndPoint.Address.MapToIPv4().ToString();
            var context = _tracingContext.CreateExitSegmentContext(eventData.Operation, host,
                new RpcCarrierHeaderCollection(eventData.Message));
            
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = "LmsRpc";
            context.Span.AddLog(LogEvent.Event("Rpc Invoke Start"));
            context.Span.AddLog(LogEvent.Message("Rpc Start"));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Peer = new StringOrIntValue(eventData.RemoteAddress);
            context.Span.AddTag(Tags.RPC_METHOD, eventData.Operation.ToString());
            context.Span.AddTag(Tags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
            context.Span.AddTag(Tags.RPC_LOCAL_ADDRESS, host);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.AfterRpcInvoker)]
        public void RpcResponse([Object] RpcResultEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Rpc Invoke End"));
            context.Span.AddLog(LogEvent.Message($"Rpc Invoke succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                                                 $"--> Message Id: {eventData.MessageId} , Name: {eventData.Operation} "));

            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcInvoker)]
        public void RpcError([Object] RpcExcetionEventData eventData)
        {
        }
    }
}