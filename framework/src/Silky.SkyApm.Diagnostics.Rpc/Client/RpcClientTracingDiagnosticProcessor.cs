using System;
using System.Linq;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Endpoint;
using Silky.SkyApm.Diagnostics.Abstraction;
using Silky.SkyApm.Diagnostics.Abstraction.Factory;
using SkyApm;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Rpc.Client
{
    public class RpcClientTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = RpcDiagnosticListenerNames.DiagnosticClientListenerName;

        private readonly TracingConfig _tracingConfig;
        private readonly ISerializer _serializer;
        private readonly ISilkySegmentContextFactory _silkySegmentContextFactory;

        public RpcClientTracingDiagnosticProcessor(IConfigAccessor configAccessor,
            ISerializer serializer,
            ISilkySegmentContextFactory silkySegmentContextFactory)
        {
            _serializer = serializer;
            _silkySegmentContextFactory = silkySegmentContextFactory;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName(RpcDiagnosticListenerNames.BeginRpcRequest)]
        public void BeginRequest([Object] RpcInvokeEventData eventData)
        {
            var localAddress = RpcContext.Context.Connection.LocalAddress;
            var serviceKey = RpcContext.Context.GetServiceKey();
            var context = _silkySegmentContextFactory.GetExitContext(eventData.ServiceEntryId);
            context.Span.AddLog(
                LogEvent.Event("Rpc Client Begin Invoke"),
                LogEvent.Message($"Rpc Client Invoke {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> ServiceKey:{serviceKey}{Environment.NewLine}" +
                                 $"--> MessageId:{eventData.MessageId}.{Environment.NewLine}" +
                                 $"--> Parameters:{_serializer.Serialize(eventData.Message.Parameters)}.{Environment.NewLine}" +
                                 $"--> InvokeAttachments:{_serializer.Serialize(RpcContext.Context.GetInvokeAttachments())}"));

            context.Span.AddTag(SilkyTags.SERVICEENTRYID, eventData.ServiceEntryId);
            context.Span.AddTag(SilkyTags.SERVICEKEY, serviceKey);
            context.Span.AddTag(SilkyTags.RPC_LOCAL_RPCENDPOINT, localAddress);
            context.Span.AddTag(SilkyTags.ISGATEWAY, RpcContext.Context.IsGateway());
        }


        [DiagnosticName(RpcDiagnosticListenerNames.SelectInvokeAddress)]
        public void SelectInvokeAddress([Object] SelectInvokeAddressEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetExitContext(eventData.ServiceEntryId);
            context.Span.AddTag(SilkyTags.RPC_SHUNTSTRATEGY, eventData.ShuntStrategy.ToString());
            context.Span.AddTag(SilkyTags.RPC_SELECTEDADDRESS, eventData.SelectedSilkyEndpoint.GetAddress());
            context.Span.AddLog(
                LogEvent.Event("Rpc Client Invoke Select Address"),
                LogEvent.Message($"Rpc Select Address  {Environment.NewLine}" +
                                 $"--> ServiceEntryId:{eventData.ServiceEntryId}.{Environment.NewLine}" +
                                 $"--> Enable Addresses:{_serializer.Serialize(eventData.EnableRpcEndpoints.Select(p => p.GetAddress()))}.{Environment.NewLine}" +
                                 $"--> SelectedAddress:{eventData.SelectedSilkyEndpoint.GetAddress()}"));
        }

        [DiagnosticName(RpcDiagnosticListenerNames.EndRpcRequest)]
        public void EndRequest([Object] RpcInvokeResultEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentExitContext(eventData.ServiceEntryId);
            if (context != null)
            {
                context.Span.AddLog(LogEvent.Event("Rpc Client Invoke End"),
                    LogEvent.Message(
                        $"Rpc Invoke Succeeded!{Environment.NewLine}" +
                        $"--> Spend Time: {eventData.ElapsedTimeMs}ms.{Environment.NewLine}" +
                        $"--> ServiceEntryId: {eventData.ServiceEntryId}.{Environment.NewLine}" +
                        $"--> MessageId: {eventData.MessageId}.{Environment.NewLine}" +
                        $"--> Result: {_serializer.Serialize(eventData.Result)}"));

                context.Span.AddTag(SilkyTags.ELAPSED_TIME, $"{eventData.ElapsedTimeMs}");
                context.Span.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                _silkySegmentContextFactory.ReleaseContext(context);
            }
        }

        [DiagnosticName(RpcDiagnosticListenerNames.ErrorRpcRequest)]
        public void RpcError([Object] RpcInvokeExceptionEventData eventData)
        {
            var context = _silkySegmentContextFactory.GetCurrentExitContext(eventData.ServiceEntryId);
            if (context != null)
            {
                context.Span?.AddTag(SilkyTags.RPC_STATUSCODE, $"{eventData.StatusCode}");
                context.Span?.ErrorOccurred(eventData.Exception, _tracingConfig);
                _silkySegmentContextFactory.ReleaseContext(context);
            }
        }
    }
}