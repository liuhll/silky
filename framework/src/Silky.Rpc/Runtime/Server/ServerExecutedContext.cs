using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerExecutedContext
    {
        public Exception Exception { get; set; }

        public object Result { get; set; }
    }
}