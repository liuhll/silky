using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerExecutingContext
    {
        public ServiceEntry ServiceEntry { get; set; }

        public Type InstanceType { get; set; }

        public object[] Parameters { get; set; }

        public string ServiceKey { get; set; }

        public Exception Exception { get; set; }
    }
}