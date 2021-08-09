using System;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.SkyApm.Diagnostics.Collections;
using Silky.Rpc.Transport;
using Silky.Rpc.Utils;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.Rpc.SkyApm.Diagnostics
{
    public class RpcServerTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticServerListenerName;

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;

        private readonly ISerializer _serializer;

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
            var carrierHeader = new SilkyCarrierHeaderCollection(RpcContext.GetContext());
            var context =
                _tracingContext.CreateEntrySegmentContext($"[ServerHandle]{eventData.ServiceId}", carrierHeader);

            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.SilkyRpc;
            context.Span.Peer = eventData.RemoteAddress;

            context.Span.AddLog(LogEvent.Event("Rpc Server Begin Handle"),
                LogEvent.Message($"Rpc Server Begin Handle {Environment.NewLine}" +
                                 $"--> ServiceId:{eventData.ServiceId}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}."));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.AddTag(SilkyTags.RPC_SERVICEID, eventData.ServiceId.ToString());
            context.Span.AddTag(SilkyTags.RPC_SERVIC_METHODENAME, eventData.ServiceMethodName);
            context.Span.AddTag(SilkyTags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
            context.Span.AddTag(SilkyTags.RPC_ATTACHMENTS, _serializer.Serialize(eventData.Message.Attachments));
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcServerHandler)]
        public void EndRpcServerHandle([Object] RpcResultEventData eventData)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null) return;
            context.Span.AddLog(LogEvent.Event("Rpc Server Handle End"),
                LogEvent.Message(
                    $"Rpc Server Handle Succeeded!{Environment.NewLine}" +
                    $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                    $"--> ServiceId:{eventData.ServiceId}.{Environment.NewLine}" +
                    $"--> MessageId:{eventData.MessageId}."));
            context.Span.AddTag(SilkyTags.ELAPSED_TIME, $"{eventData.ElapsedTimeMs}");
            context.Span.AddTag(SilkyTags.RPC_RESULT, _serializer.Serialize(eventData.Result));
            context.Span.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcServerHandler)]
        public void ErrorRpcServerHandle([Object] RpcExceptionEventData eventData)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
    }
}