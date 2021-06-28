using System.Collections.Generic;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.Rpc.Messages
{
    public class RemoteResultMessage : IRemoteMessage
    {
        public string ExceptionMessage { get; set; }

        public StatusCode StatusCode { get; set; } = StatusCode.Success;

        public ValidError[] ValidateErrors { get; set; }

        public object Result { get; set; }
    }


}