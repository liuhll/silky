using System;
using System.Collections.Concurrent;
using Silky.Lms.Core.Serialization;
using Silky.Lms.Rpc.Diagnostics;
using Silky.Lms.Rpc.SkyApm.Diagnostics.Collections;
using Silky.Lms.Rpc.Transport;
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
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;

        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;

        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;

        public RpcClientTracingDiagnosticProcessor(ITracingContext tracingContext,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            IConfigAccessor configAccessor,
            ISerializer serializer)
        {
            _tracingContext = tracingContext;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
        }

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcRequest)]
        public void BeginRequest([Object] RpcInvokeEventData eventData)
        {
         //   var host = NetUtil.GetRpcAddressModel().IPEndPoint.ToString();
            var context = _tracingContext.CreateExitSegmentContext($"{eventData.RemoteAddress}{eventData.ServiceId}", eventData.RemoteAddress,
                new SilkyCarrierHeaderCollection(RpcContext.GetContext()));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.LmsRpc;
            context.Span.AddLog(LogEvent.Event("Rpc Client BeginRequest"),
                LogEvent.Message($"Request starting {eventData.ServiceId}"));
            context.Span.AddTag(SilkyTags.RPC_SERVICEID, eventData.ServiceId.ToString());
            context.Span.AddTag(SilkyTags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcRequest)]
        public void EndRequest([Object] RpcResultEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;

            context.Span.AddLog(LogEvent.Event("Rpc Invoke End"));
            context.Span.AddLog(LogEvent.Message($"Rpc Invoke succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                                                 $"--> Message Id: {eventData.MessageId} , ServiceId: {eventData.ServiceId} "));

            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcRequest)]
        public void RpcError([Object] RpcExcetionEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
    }
}