using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Primitives;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerManager
    {
        void Update([NotNull] ServerDescriptor serverDescriptor);
        
        void UpdateAll([NotNull] ServerDescriptor[] serverDescriptors);

        void Update([NotNull] IServer server);

        void Remove(string hostName);

        ServiceDescriptor GetServiceDescriptor(string serviceId);

        ServerDescriptor GetServerDescriptor(string hostName);

        IServer GetServer(string hostName);

        ServerDescriptor[]? ServerDescriptors { get; }

        IServer[]? Servers { get; }

        ISilkyEndpoint[]? GetRpcEndpoints(string serviceId, ServiceProtocol serviceProtocol);

        IServer GetSelfServer();

        ServiceEntryDescriptor GetServiceEntryDescriptor(string serviceEntryId);

        ServiceEntryDescriptor GetServiceEntryDescriptor(string api, HttpMethod httpMethod);

        event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;

        event OnUpdateRpcEndpoint OnUpdateRpcEndpoint;
        
        IChangeToken GetChangeToken();
        
    }
}