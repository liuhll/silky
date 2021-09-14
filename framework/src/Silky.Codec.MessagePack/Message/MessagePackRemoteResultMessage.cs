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
            Result = remoteResultMessage.Result;
            ValidateErrors = remoteResultMessage.ValidateErrors;
        }

        public MessagePackRemoteResultMessage()
        {
        }

        [Key(1)] public string ServiceEntryId { get; set; }
        [Key(2)] public string ExceptionMessage { get; set; }

        [Key(3)] public StatusCode StatusCode { get; set; } = StatusCode.Success;

        [Key(4)] public object Result { get; set; }

        [Key(5)] public ValidError[] ValidateErrors { get; set; }


        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ServiceEntryId = ServiceEntryId,
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Result = Result,
                ValidateErrors = ValidateErrors
            };
        }
    }
}