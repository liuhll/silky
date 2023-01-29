using System.Collections.Generic;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServer
    {
        string HostName { get; }

        ISilkyEndpoint? RpcEndpoint { get; }
        
        ISilkyEndpoint? WsEndpoint { get; }
        
        ISilkyEndpoint? WebEndpoint { get; }

        ICollection<ISilkyEndpoint> Endpoints { get; set; }

        ICollection<ServiceDescriptor> Services { get; set; }
        
    }
}