using System;
using System.Collections.Concurrent;
using Silky.Core.Convertible;
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
            _serializer = serializer;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcRequest)]
        public void BeginRequest([Object] RpcInvokeEventData eventData)
        {
            var clientAddress = RpcContext.Context.GetClientAddress();
            var serverAddress = RpcContext.Context.GetServerAddress();
            var context = _tracingContext.CreateExitSegmentContext($"[ClientInvoke]{eventData.ServiceEntryId}",
                serverAddress, new SilkyCarrierHeaderCollection(RpcContext.Context));

            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.Component = Components.SilkyRpc;
            context.Span.AddLog(LogEvent.Event("Rpc Client Begin Invoke"),
                LogEvent.Message($"Rpc Client Invoke {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}."));

            context.Span.AddTag(SilkyTags.RPC_SERVICEENTRYID, eventData.ServiceEntryId.ToString());
            context.Span.AddTag(SilkyTags.RPC_PARAMETERS, _serializer.Serialize(eventData.Message.Parameters));
            context.Span.AddTag(SilkyTags.RPC_ATTACHMENTS, _serializer.Serialize(eventData.Message.Attachments));
            context.Span.AddTag(SilkyTags.RPC_CLIENT_ADDRESS, clientAddress);
            context.Span.AddTag(SilkyTags.RPC_SERVER_ADDRESS, serverAddress);
            context.Span.AddTag(SilkyTags.ISGATEWAY, RpcContext.Context.IsGateway());
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcRequest)]
        public void EndRequest([Object] RpcResultEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null) return;
            context.Span.AddLog(LogEvent.Event("Rpc Client Invoke End"),
                LogEvent.Message(
                    $"Rpc Invoke Succeeded!{Environment.NewLine}" +
                    $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                    $"--> ServiceEntryId: {eventData.ServiceEntryId}.{Environment.NewLine}" +
                    $"--> MessageId: {eventData.MessageId}."));
            context.Span.AddTag(SilkyTags.ELAPSED_TIME, $"{eventData.ElapsedTimeMs}");
            context.Span.AddTag(SilkyTags.RPC_RESULT, _serializer.Serialize(eventData.Result));
            context.Span.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
            _tracingContext.Release(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcRequest)]
        public void RpcError([Object] RpcExceptionEventData eventData)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context != null)
            {
                context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
                _tracingContext.Release(context);
            }
        }
    }
}