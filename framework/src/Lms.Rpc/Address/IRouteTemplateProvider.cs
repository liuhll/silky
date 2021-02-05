using Lms.Rpc.Runtime.Server.ServiceEntry;

namespace Lms.Rpc.Address
{
    public interface IRouteTemplateProvider
    {
        string Template { get; }
        
        ServiceProtocol ServiceProtocol { get; }
    }
}