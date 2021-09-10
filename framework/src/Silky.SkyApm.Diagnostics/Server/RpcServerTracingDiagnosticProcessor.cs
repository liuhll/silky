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
            var clientAddress = RpcContext.Context.GetClientAddress();
            var serverAddress = RpcContext.Context.GetServerAddress();
            var carrierHeader = new SilkyCarrierHeaderCollection(RpcContext.Context);
            var context =
                _tracingContext.CreateEntrySegmentContext($"[ServerHandle]{eventData.ServiceEntryId}", carrierHeader);

            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.SilkyRpc;
            context.Span.Peer = serverAddress;

            context.Span.AddLog(LogEvent.Event("Rpc Server Begin Handle"),
                LogEvent.Message($"Rpc Server Begin Handle {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}."));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.AddTag(SilkyTags.RPC_SERVICEENTRYID, eventData.ServiceEntryId.ToString());
            context.Span.AddTag(SilkyTags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
            context.Span.AddTag(SilkyTags.RPC_ATTACHMENTS, _serializer.Serialize(eventData.Message.Attachments));
            context.Span.AddTag(SilkyTags.RPC_CLIENT_ADDRESS, clientAddress);
            context.Span.AddTag(SilkyTags.RPC_SERVER_ADDRESS, serverAddress);
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
                    $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
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