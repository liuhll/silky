using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Routing.Builder.Internal;

internal abstract class SilkyServiceEntryEndpointDataSourceBase : SilkyEndpointDataSourceBase
{
    private IChangeToken? _changeToken;
    private List<Endpoint>? _endpoints;
    private CancellationTokenSource? _cancellationTokenSource;
    protected readonly IServiceEntryManager _serviceEntryManager;
    private readonly List<Action<EndpointBuilder>> Conventions;
    private readonly ServiceEntryEndpointFactory _serviceEntryEndpointFactory;
    private readonly object Lock = new();

    protected SilkyServiceEntryEndpointDataSourceBase(IServiceEntryManager serviceEntryManager,
        ServiceEntryEndpointFactory serviceEntryEndpointFactory)
    {
        _serviceEntryManager = serviceEntryManager;
        _serviceEntryEndpointFactory = serviceEntryEndpointFactory;
        Conventions = new List<Action<EndpointBuilder>>();
        DefaultBuilder = new SilkyRpcServiceEndpointConventionBuilder(Lock, Conventions);
    }
    
    private List<Endpoint> CreateEndpoints(IReadOnlyList<ServiceEntry> serviceEntries,
        IReadOnlyList<Action<EndpointBuilder>> conventions)
    {
        var endpoints = new List<Endpoint>();
        var routeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var serviceEntry in serviceEntries)
        {
            _serviceEntryEndpointFactory.AddEndpoints(endpoints, routeNames, serviceEntry, conventions);
        }

        return endpoints;
    }


    
    protected override void UpdateEndpoints()
    {
        lock (Lock)
        {
            var serviceEntries = GetEntries();
            var endpoints = CreateEndpoints(serviceEntries, Conventions);

            // See comments in DefaultActionDescriptorCollectionProvider. These steps are done
            // in a specific order to ensure callers always see a consistent state.

            // Step 1 - capture old token
            var oldCancellationTokenSource = _cancellationTokenSource;

            // Step 2 - update endpoints
            _endpoints = endpoints;

            // Step 3 - create new change token
            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            // Step 4 - trigger old token
            oldCancellationTokenSource?.Cancel();
        }
    }

    protected abstract ServiceEntry[] GetEntries();
}