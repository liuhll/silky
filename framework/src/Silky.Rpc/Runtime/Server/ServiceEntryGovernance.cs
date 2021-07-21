using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {
        public bool ProhibitExtranet{ get; set; } = false;
    }
}