using System.Collections.Generic;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServer
    {
        string HostName { get; }

        ICollection<IRpcEndpoint> Endpoints { get; set; }

        ICollection<ServiceDescriptor> Services { get; set; }
        
        void RemoveEndpoint(IRpcEndpoint endpoint);
    }
}