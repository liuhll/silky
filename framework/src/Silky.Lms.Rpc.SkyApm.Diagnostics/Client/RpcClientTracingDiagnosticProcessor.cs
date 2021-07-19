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
    public class RpcClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticClientListenerName;
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;

        public RpcClientTracingDiagnosticProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            IConfigAccessor configAccessor,
            ISerializer serializer)
        {
            _tracingContext = tracingContext;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
        }

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcRequest)]
        public void BeginRequest([Object] RpcInvokeEventData eventData)
        {
            var host = NetUtil.GetRpcAddressModel().IPEndPoint.ToString();
            var context = _tracingContext.CreateExitSegmentContext($"{host}{eventData.Operation}", host,
                new RpcCarrierHeaderCollection(eventData.Message));

            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.LmsRpc;
            context.Span.AddLog(LogEvent.Event("Rpc Client BeginRequest"), LogEvent.Message($"Request starting {eventData.Operation}"));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Peer = new StringOrIntValue(host);
            context.Span.AddTag(Tags.RPC_SERVICEID, eventData.Operation.ToString());
            context.Span.AddTag(Tags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
            context.Span.AddTag(Tags.RPC_LOCAL_ADDRESS, host);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcRequest)]
        public void EndRequest([Object] RpcResultEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Rpc Invoke End"));
            context.Span.AddLog(LogEvent.Message($"Rpc Invoke succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                                                 $"--> Message Id: {eventData.MessageId} , Name: {eventData.Operation} "));

            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcRequest)]
        public void RpcError([Object] RpcExcetionEventData eventData)
        {
        }
        

    }
}