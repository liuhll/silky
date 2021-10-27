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

        ServerDescriptor GetServerDescriptor(string hostName);

        IServer GetServer(string hostName);

        IReadOnlyList<ServerDescriptor> ServerDescriptors { get; }

        IReadOnlyList<IServer> Servers { get; }

        IRpcEndpoint[] GetRpcEndpoints(string serviceId, ServiceProtocol serviceProtocol);

        IServer GetSelfServer();
        
        ServiceEntryDescriptor GetServiceEntryDescriptor(string serviceEntryId);

        event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;

        event OnUpdateRpcEndpoint OnUpdateRpcEndpoint;
    }
}