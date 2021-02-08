using Lms.Core.Exceptions;

namespace Lms.Rpc.Messages
{
    public class RemoteResultMessage : IRemoteMessage
    {
        public string ExceptionMessage { get; set; }

        public StatusCode StatusCode { get; set; } = StatusCode.Success;

        public object Result { get; set; }
    }
}