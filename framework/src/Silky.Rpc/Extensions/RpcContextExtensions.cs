using System.Collections.Generic;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Auditing;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Extensions
{
    public static class RpcContextExtensions
    {
        public static void SetRcpInvokeAddressInfo(this RpcContext rpcContext, RpcEndpointDescriptor serverRpcEndpoint)
        {
            rpcContext
                .SetInvokeAttachment(AttachmentKeys.SelectedServerHost, serverRpcEndpoint.Host);
            rpcContext
                .SetInvokeAttachment(AttachmentKeys.SelectedServerPort, serverRpcEndpoint.Port.ToString());
            rpcContext
                .SetInvokeAttachment(AttachmentKeys.SelectedServerServiceProtocol,
                    serverRpcEndpoint.ServiceProtocol.ToString());


            var localRpcEndpointDescriptor = EndpointHelper.GetLocalRpcEndpoint();
            rpcContext.SetInvokeAttachment(AttachmentKeys.ClientHost, localRpcEndpointDescriptor.Host);
            rpcContext.SetInvokeAttachment(AttachmentKeys.ClientServiceProtocol, localRpcEndpointDescriptor.ServiceProtocol.ToString());
            rpcContext.SetInvokeAttachment(AttachmentKeys.ClientPort, localRpcEndpointDescriptor.Port.ToString());
            if (RpcContext.Context.GetLocalHost().IsNullOrEmpty())
            {
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalAddress, localRpcEndpointDescriptor.Host);
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalPort, localRpcEndpointDescriptor.Port.ToString());
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalServiceProtocol,
                    localRpcEndpointDescriptor.ServiceProtocol.ToString());
            }
        }
        
        public static void SetAuditingActionLog(this RpcContext rpcContext, AuditLogActionInfo auditLogActionInfo)
        {
            var auditSerializer = EngineContext.Current.Resolve<IAuditSerializer>();
            var auditLogsValue = RpcContext.Context.GetResultAttachment(AttachmentKeys.AuditActionLog);
            IList<string> auditLogs;
            auditLogs = auditLogsValue != null ? auditLogsValue.ConventTo<IList<string>>() : new List<string>();
            auditLogs.Add(auditSerializer.Serialize(auditLogActionInfo));
            rpcContext.SetResultAttachment(AttachmentKeys.AuditActionLog, auditLogs);
        }
    }
}