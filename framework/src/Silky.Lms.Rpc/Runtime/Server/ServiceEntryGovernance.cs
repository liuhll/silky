using Silky.Lms.Rpc.Configuration;

namespace Silky.Lms.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {
        public bool ProhibitExtranet{ get; set; } = false;
    }
}