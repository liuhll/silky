using Lms.Rpc.Configuration;

namespace Lms.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {
        public bool ProhibitExtranet{ get; set; } = false;
    }
}