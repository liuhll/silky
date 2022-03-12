using System.Linq;
using Silky.Core.Exceptions;
using ProtoBuf;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec.Message
{
    [ProtoContract]
    public class ProtoBufRemoteResultMessage
    {
        public ProtoBufRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            ServiceEntryId = remoteResultMessage.ServiceEntryId;
            ExceptionMessage = remoteResultMessage.ExceptionMessage;
            StatusCode = remoteResultMessage.StatusCode;
            Status = remoteResultMessage.Status;
            Result = remoteResultMessage.Result == null ? null : new DynamicItem(remoteResultMessage.Result);
            ValidateErrors = remoteResultMessage.ValidateErrors?.Select(i => new DynamicItem(i)).ToArray();
            Attachments = remoteResultMessage.Attachments?.Select(i => new ParameterItem(i)).ToArray();
        }

        public ProtoBufRemoteResultMessage()
        {
        }

        [ProtoMember(1)] public string ServiceEntryId { get; set; }
        [ProtoMember(2)] public string ExceptionMessage { get; set; }

        [ProtoMember(3)] public StatusCode StatusCode { get; set; } = StatusCode.Success;
        
        [ProtoMember(4)] public int Status { get; set; } = (int)StatusCode.Success;

        [ProtoMember(5)] public DynamicItem Result { get; set; }

        [ProtoMember(6)] public DynamicItem[] ValidateErrors { get; set; }

        [ProtoMember(7)] public ParameterItem[] Attachments { get; set; }

        public RemoteResultMessage GetMessage()
        {
            return new()
            {
                ServiceEntryId = ServiceEntryId,
                ExceptionMessage = ExceptionMessage,
                StatusCode = StatusCode,
                Status = Status,
                Result = Result?.Get(),
                ValidateErrors = ValidateErrors?.Select(p => (ValidError)p.Get()).ToArray(),
                Attachments = Attachments?.ToDictionary(i => i.Key, i => i.Value?.Get()),
            };
        }
    }
}