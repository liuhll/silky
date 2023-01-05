using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Transport.Messages
{
    public class RemoteInvokeMessage : IRemoteMessage
    {
        public string ServiceEntryId { get; set; }

        public string ServiceId { get; set; }

        public object[] Parameters { get; set; }

        public IDictionary<string, object> DictParameters { get; set; }
        
        public IDictionary<ParameterFrom, object> HttpParameters { get; set; }

        public ParameterType ParameterType { get; set; }

        public IDictionary<string, string> Attachments { get; set; }
    }
}