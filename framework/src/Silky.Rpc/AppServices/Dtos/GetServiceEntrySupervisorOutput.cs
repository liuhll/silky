using System.Collections.Generic;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices.Dtos
{
    public class GetServiceEntrySupervisorOutput
    {
        public IReadOnlyCollection<ServiceEntryHandleInfo> ServiceEntryHandleInfo { get; set; }

        public IReadOnlyCollection<ServiceEntryInvokeInfo> ServiceEntryInvokeInfo { get; set; }
    }
}