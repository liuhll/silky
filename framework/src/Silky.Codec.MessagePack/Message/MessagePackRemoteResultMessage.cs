using Silky.Core.Exceptions;
using Silky.Rpc.Messages;
using MessagePack;

namespace Silky.Codec.Message
{
    [MessagePackObject]
    public class MessagePackRemoteResultMessage
    {
        public MessagePackRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            ExceptionMessage = remoteResultMessage.ExceptionMessage;
            StatusCode = remoteResultMessage.StatusCode;
            Result = remoteResultMessage.Result;
            ValidateErrors = remoteResultMessage.ValidateErrors;
        }

        public MessagePackRemoteResultMessage()
        {
        }

        [Key(1)] public string ExceptionMessage { get; set; }

        [Key(2)] public StatusCode StatusCode { get; set; } = StatusCode.Success;

        [Key(3)] public object Result { get; set; }
        
        [Key(4)] public ValidError[] ValidateErrors { get; set; }

        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Result = Result,
                ValidateErrors = ValidateErrors
            };
        }
    }
}