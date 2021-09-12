using System;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.SkyApm.Diagnostics.Rpc.Factory;
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
        private readonly ISilkyRpcSegmentContextFactory _silkyRpcSegmentContextFactory;

        public RpcServerTracingDiagnosticProcessor(IConfigAccessor configAccessor,
            ISilkyRpcSegmentContextFactory silkyRpcSegmentContextFactory,
            ISerializer serializer)
        {
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _serializer = serializer;
            _silkyRpcSegmentContextFactory = silkyRpcSegmentContextFactory;
        }

        #region Rpc Servce Handle

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcServerHandler)]
        public void BeginRpcServerHandle([Object] RpcInvokeEventData eventData)
        {
            var clientAddress = RpcContext.Context.GetClientAddress();
            var serverAddress = RpcContext.Context.GetServerAddress();
            var serviceKey = RpcContext.Context.GetServerKey();
            var context = _silkyRpcSegmentContextFactory.GetEntryContext(eventData.ServiceEntryId);
            context.Span.AddLog(
                LogEvent.Event("Rpc Server Begin Handle"),
                LogEvent.Message($"Rpc Server Begin Handle {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> ServiceKey:{serviceKey}.{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}." +
                                 $"--> Parameters:{_serializer.Serialize(eventData.Message.Parameters)}.{Environment.NewLine}" +
                                 $"--> Attachments:{_serializer.Serialize(eventData.Message.Attachments)}"));

            context.Span.AddTag(SilkyTags.RPC_SERVICEENTRYID, eventData.ServiceEntryId);
            context.Span.AddTag(SilkyTags.SERVICEKEY, serviceKey);
            context.Span.AddTag(SilkyTags.RPC_CLIENT_ADDRESS, clientAddress);
            context.Span.AddTag(SilkyTags.RPC_SERVER_ADDRESS, serverAddress);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcServerHandler)]
        public void EndRpcServerHandle([Object] RpcInvokeResultEventData eventData)
        {
            var context = _silkyRpcSegmentContextFactory.GetEntryContext(eventData.ServiceEntryId);
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
            _silkyRpcSegmentContextFactory.ReleaseContext(context);
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcServerHandler)]
        public void ErrorRpcServerHandle([Object] RpcInvokeExceptionEventData eventData)
        {
            var context = _silkyRpcSegmentContextFactory.GetEntryContext(eventData.ServiceEntryId);
            context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
            context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
            _silkyRpcSegmentContextFactory.ReleaseContext(context);
        }

        #endregion
    }
}