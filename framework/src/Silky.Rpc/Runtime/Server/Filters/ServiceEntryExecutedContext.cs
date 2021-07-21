using System;

namespace Silky.Rpc.Runtime.Server.Filters
{
    public class ServiceEntryExecutedContext
    {
        public Exception Exception { get; set; }

        public object Result { get; set; }
    }
}