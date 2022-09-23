using System.Collections.Generic;
using Silky.Core.Exceptions;
using MessagePack;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec.Message
{
    [MessagePackObject]
    public class MessagePackRemoteResultMessage
    {
        public MessagePackRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            ServiceEntryId = remoteResultMessage.ServiceEntryId;
            ExceptionMessage = remoteResultMessage.ExceptionMessage;
            StatusCode = remoteResultMessage.StatusCode;
            Status = remoteResultMessage.Status;
            Result = remoteResultMessage.Result;
            ValidateErrors = remoteResultMessage.ValidateErrors;
            Attachments = remoteResultMessage.Attachments;
        }

        public MessagePackRemoteResultMessage()
        {
        }

        [Key(1)] public string ServiceEntryId { get; set; }
        [Key(2)] public string ExceptionMessage { get; set; }

        [Key(3)] public StatusCode StatusCode { get; set; } = StatusCode.Success;
        
        [Key(4)] public int Status{ get; set; } = (int)StatusCode.Success;
        [Key(5)] public object Result { get; set; }

        [Key(6)] public ValidError[] ValidateErrors { get; set; }

        [Key(7)] public IDictionary<string, string> Attachments { get; set; }


        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ServiceEntryId = ServiceEntryId,
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Status = Status,
                Result = Result,
                ValidateErrors = ValidateErrors,
                Attachments = Attachments
            };
        }
    }
}