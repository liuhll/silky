using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {
        public bool ProhibitExtranet { get; set; } = false;

        public bool IsAllowAnonymous { get; set; } = false;
        
    }
}