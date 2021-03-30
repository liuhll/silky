using System.Collections.Generic;
using Lms.Core.Exceptions;

namespace Lms.Rpc.Messages
{
    public class RemoteResultMessage : IRemoteMessage
    {
        public string ExceptionMessage { get; set; }

        public StatusCode StatusCode { get; set; } = StatusCode.Success;

        public IEnumerable<ValidateError> ValidateErrors { get; set; }

        public object Result { get; set; }
    }


}