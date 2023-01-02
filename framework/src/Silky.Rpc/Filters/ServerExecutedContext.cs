using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerExecutedContext
    {
        public Exception Exception { get; set; }

        public object Result { get; set; }
        public ServiceEntry ServiceEntry { get; set; }
    }
}