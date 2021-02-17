
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Address
{
    public interface IRouteTemplateProvider
    {
        string Template { get; }
        
        ServiceProtocol ServiceProtocol { get; }
        
        bool MultipleServiceKey { get; }
    }
}