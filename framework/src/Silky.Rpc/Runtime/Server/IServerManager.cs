using System.Collections.Generic;
using JetBrains.Annotations;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerManager
    {
        void Update([NotNull] ServerDescriptor serverDescriptor);

        void Remove(string hostName);

        ServiceDescriptor GetServiceDescriptor(string serviceId);

        IReadOnlyList<ServerDescriptor> ServerDescriptors { get; }

        IRpcEndpoint[] GetRpcEndpoints(string serviceId, ServiceProtocol serviceProtocol);

        IServer GetSelfServer();
        
        event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;
    }
}