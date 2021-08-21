using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryDetailOutput : GetServiceEntryOutput
    {

        public ServiceEntryGovernance GovernanceOptions { get; set; }
     
    }
    
}