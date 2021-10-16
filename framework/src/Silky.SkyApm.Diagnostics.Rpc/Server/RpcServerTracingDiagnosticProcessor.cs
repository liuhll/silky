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

namespace Silky.SkyApm.Diagnostics.Rpc.Server
{
    public class RpcServerTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticServerListenerName;
        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;
        private readonly ISilkySegmentContextFactory _silkySegmentContextFactory;

        public RpcServerTracingDiagnosticProcessor(IConfigAccessor configAccessor,
            ISilkySegmentContextFactory silkySegmentContextFactory,
            ISerializer serializer)
        {
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
            _silkySegmentContextFactory = silkySegmentContextFactory;
        }

        #region Rpc Servce Handle

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcServerHandler)]
        public void BeginRpcServerHandle([Object] RpcInvokeEventData eventData)
        {
            var rpcConnection = RpcContext.Context.Connection;
            var localEndpoint = rpcConnection.LocalAddress;
            var clientAddress = rpcConnection.ClientAddress;
            var serviceKey = RpcContext.Context.GetServiceKey();
            var context = _silkySegmentContextFactory.GetEntryContext(eventData.ServiceEntryId);
            context.Span.AddLog(
                LogEvent.Event("Rpc Server Begin Handle"),
                LogEvent.Message($"Rpc Server Begin Handle {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> ServiceKey:{serviceKey}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}." +
                                 $"--> Parameters:{_serializer.Serialize(eventData.Message.Parameters)}.{Environment.NewLine}" +
                                 $"--> Attachments:{_serializer.Serialize(eventData.Message.Attachments)}"));

            context.Span.AddTag(SilkyTags.SERVICEENTRYID, eventData.ServiceEntryId);
            context.Span.AddTag(SilkyTags.SERVICEKEY, serviceKey);
            context.Span.AddTag(SilkyTags.RPC_CLIENT_ENDPOINT, clientAddress);
            context.Span.AddTag(SilkyTags.RPC_REMOTE_PORT, rpcConnection.RemotePort?.ToString());
            context.Span.AddTag(SilkyTags.RPC_LOCAL_RPCENDPOINT, localEndpoint);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcServerHandler)]
        public void EndRpcServerHandle([Object] RpcInvokeResultEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentEntryContext(eventData.ServiceEntryId);
            if (context != null)
            {
                context.Span.AddLog(
                    LogEvent.Event("Rpc Server Handle End"),
                    LogEvent.Message(
                        $"Rpc Server Handle Succeeded!{Environment.NewLine}" +
                        $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                        $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                        $"--> MessageId: {eventData.MessageId}.{Environment.NewLine}" +
                        $"--> Result: {_serializer.Serialize(eventData.Result)}"));
                context.Span.AddTag(SilkyTags.ELAPSED_TIME, $"{eventData.ElapsedTimeMs}");
                context.Span.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                _silkySegmentContextFactory.ReleaseContext(context);
            }
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcServerHandler)]
        public void ErrorRpcServerHandle([Object] RpcInvokeExceptionEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentEntryContext(eventData.ServiceEntryId);
            if (context != null)
            {
                context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
            }
        }

        #endregion
    }
}