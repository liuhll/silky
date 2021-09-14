using Silky.Core.Exceptions;

namespace Silky.Rpc.Transport.Messages
{
    public class RemoteResultMessage : IRemoteMessage
    {
        public string ExceptionMessage { get; set; }

        public string ServiceEntryId { get; set; }

        public StatusCode StatusCode { get; set; } = StatusCode.Success;

        public ValidError[] ValidateErrors { get; set; }

        public object Result { get; set; }
    }
}