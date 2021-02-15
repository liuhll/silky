using Lms.Core.Exceptions;
using Lms.Rpc.Messages;
using MessagePack;

namespace Lms.Codec.Message
{
    [MessagePackObject]
    public class MessagePackRemoteResultMessage
    {
        public MessagePackRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            ExceptionMessage = remoteResultMessage.ExceptionMessage;
            StatusCode = remoteResultMessage.StatusCode;
            Result = remoteResultMessage.Result;
        }

        public MessagePackRemoteResultMessage()
        {
        }

        [Key(1)] public string ExceptionMessage { get; set; }

        [Key(2)] public StatusCode StatusCode { get; set; } = StatusCode.Success;

        [Key(3)] public object Result { get; set; }

        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Result = Result
            };
        }
    }
}