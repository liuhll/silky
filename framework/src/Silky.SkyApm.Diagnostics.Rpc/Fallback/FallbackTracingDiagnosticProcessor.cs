using System;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Diagnostics;
using Silky.SkyApm.Diagnostics.Abstraction;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using SkyApm;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Rpc.Fallback
{
    public class FallbackTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticFallbackListenerName;

        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;
        private readonly ISilkySegmentContextFactory _silkySegmentContextFactory;

        public FallbackTracingDiagnosticProcessor(IConfigAccessor configAccessor,
            ISerializer serializer,
            ISilkySegmentContextFactory silkySegmentContextFactory)
        {
            _serializer = serializer;
            _silkySegmentContextFactory = silkySegmentContextFactory;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(RpcDiagnosticListenerNames.RpcFallbackBegin)]
        public void RpcFallbackBegin([Object] FallbackEventData eventData)
        {
            var rpcConnection = RpcContext.Context.Connection;
            var localEndpoint = rpcConnection.LocalAddress;
            var clientAddress = rpcConnection.ClientAddress;
            var context = _silkySegmentContextFactory.GetCurrentContext(GetOperationName(eventData.ServiceEntryId));
            context.Span.AddLog(
                LogEvent.Event("Fallback Begin Handle"),
                LogEvent.Message($"Rpc Fallback Begin Handle {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}.{Environment.NewLine}" +
                                 $"--> Parameters:{_serializer.Serialize(eventData.Parameters)}.{Environment.NewLine}" +
                                 $"--> FallbackType:{eventData.FallbackProvider.Type.FullName}.{Environment.NewLine}" +
                                 $"--> FallbackType Method:{eventData.FallbackProvider.MethodName}.{Environment.NewLine}" +
                                 $"--> FallbackType Weight:{eventData.FallbackProvider.Weight}"));

            context.Span.AddTag(SilkyTags.SERVICEENTRYID, eventData.ServiceEntryId);
            context.Span.AddTag(SilkyTags.RPC_CLIENT_ENDPOINT, clientAddress);
            context.Span.AddTag(SilkyTags.RPC_REMOTE_PORT, rpcConnection.RemotePort?.ToString());
            context.Span.AddTag(SilkyTags.RPC_LOCAL_RPCENDPOINT, localEndpoint);
            context.Span.AddTag(SilkyTags.FALLBACK_EXEC_TYPE, eventData.FallbackExecType.ToString());
            context.Span.AddTag(SilkyTags.FALLBACK_TYPE, eventData.FallbackProvider.Type.FullName);
            context.Span.AddTag(SilkyTags.FALLBACK_METHOD, eventData.FallbackProvider.MethodName);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.RpcFallbackEnd)]
        public void RpcFallbackEnd([Object] FallbackResultEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentContext(GetOperationName(eventData.ServiceEntryId));
            context.Span.AddLog(
                LogEvent.Event("Rpc Fallback Handle End"),
                LogEvent.Message(
                    $"Rpc Fallback Handle Succeeded!{Environment.NewLine}" +
                    $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                    $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                    $"--> MessageId: {eventData.MessageId}.{Environment.NewLine}" +
                    $"--> Result: {_serializer.Serialize(eventData.Result)}"));
            context.Span.AddTag(SilkyTags.ELAPSED_TIME, $"{eventData.ElapsedTimeMs}");
            _silkySegmentContextFactory.ReleaseContext(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.RpcFallbackError)]
        public void RpcFallbackError([Object] FallbackExceptionEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentContext(GetOperationName(eventData.ServiceEntryId));
            context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
            context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
        }

        private string GetOperationName(string serviceEntryId)
        {
            return $"[Fallback]{serviceEntryId}";
        }
    }
}