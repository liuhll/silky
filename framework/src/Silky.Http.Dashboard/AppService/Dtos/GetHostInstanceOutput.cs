using System;
using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetHostInstanceOutput
    {
        public string HostName { get; set; }

        public string Host { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public bool IsHealth { get; set; }

        public bool IsEnable { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public DateTime? LastDisableTime { get; set; }
    }
}