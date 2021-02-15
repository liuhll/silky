using Lms.Core.Exceptions;
using Lms.Rpc.Messages;
using ProtoBuf;

namespace Lms.Codec.Message
{
    [ProtoContract]
    public class ProtoBufRemoteResultMessage
    {
        public ProtoBufRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            ExceptionMessage = remoteResultMessage.ExceptionMessage;
            StatusCode = remoteResultMessage.StatusCode;
            Result = remoteResultMessage.Result == null ? null : new DynamicItem(remoteResultMessage.Result);
        }

        public ProtoBufRemoteResultMessage()
        {
        }

        [ProtoMember(1)] public string ExceptionMessage { get; set; }

        [ProtoMember(2)] public StatusCode StatusCode { get; set; } = StatusCode.Success;

        [ProtoMember(3)] public DynamicItem Result { get; set; }

        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Result = Result?.Get()
            };
        }
    }
}