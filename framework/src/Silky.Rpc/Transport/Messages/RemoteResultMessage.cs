using System.Collections.Generic;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Transport.Messages
{
    public class RemoteResultMessage : IRemoteMessage
    {
        public string ExceptionMessage { get; set; }

        public string ServiceEntryId { get; set; }

        public StatusCode StatusCode { get; set; } = StatusCode.Success;

        public int Status => (int)StatusCode;

        public ValidError[] ValidateErrors { get; set; }

        public object Result { get; set; }

        public IDictionary<string, string> Attachments { get; set; }
    }
}