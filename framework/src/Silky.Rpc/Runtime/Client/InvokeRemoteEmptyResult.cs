using Silky.Core.Exceptions;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client;

public class InvokeRemoteEmptyResult : RemoteResultMessage
{
    public InvokeRemoteEmptyResult(ClientInvokeContext context)
    {
        ServiceEntryId = context.RemoteInvokeMessage.ServiceEntryId;
        StatusCode = StatusCode.NoContent;
        Attachments = context.RemoteInvokeMessage.Attachments;
        TransAttachments = context.RemoteInvokeMessage.TransAttachments;
        ExceptionMessage = "Remote RemoteResult Empty!";
    }
}