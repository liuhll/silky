using System;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerInstanceDetailInfo
    {
        public string Address { get; set; }

        public string HostName { get; set; }

        public DateTime StartTime { get; set; }

        public ServerInstanceHandleInfo HandleInfo { get; set; }

        public ServerInstanceInvokeInfo InvokeInfo { get; set; }
    }
}