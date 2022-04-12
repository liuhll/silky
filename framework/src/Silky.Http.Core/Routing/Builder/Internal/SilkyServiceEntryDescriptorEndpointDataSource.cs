using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Routing.Builder.Internal;

public class SilkyServiceEntryDescriptorEndpointDataSource : EndpointDataSource
{
    private IChangeToken? _changeToken;
    private List<Endpoint>? _endpoints;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly IServerManager _serverManager;
    private readonly List<Action<EndpointBuilder>> Conventions;
    private readonly ServiceEntryDescriptorEndpointFactory _serviceEntryDescriptorEndpointFactory;
    private readonly object Lock = new();

    public SilkyServiceEntryDescriptorEndpointDataSource(IServerManager serverManager,
        ServiceEntryDescriptorEndpointFactory serviceEntryDescriptorEndpointFactory)
    {
        _serverManager = serverManager;
        _serviceEntryDescriptorEndpointFactory = serviceEntryDescriptorEndpointFactory;
        Conventions = new List<Action<EndpointBuilder>>();
        DefaultBuilder = new ServiceEntryDescriptorEndpointConventionBuilder(Lock, Conventions);
    }

    public ServiceEntryDescriptorEndpointConventionBuilder DefaultBuilder { get; }

    public override IChangeToken GetChangeToken()
    {
        Initialize();
        Debug.Assert(_changeToken != null);
        Debug.Assert(_endpoints != null);
        return _changeToken;
    }

    private void Initialize()
    {
        if (_endpoints == null)
        {
            lock (Lock)
            {
                if (_endpoints == null)
                {
                    UpdateEndpoints();
                }
            }
        }
    }

    private void UpdateEndpoints()
    {
        lock (Lock)
        {
            var serviceEntryDescriptors =
                _serverManager.Servers.SelectMany(p => p.Services).SelectMany(p => p.ServiceEntries).ToArray();

            var endpoints = CreateEndpoints(serviceEntryDescriptors, Conventions);

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

    private List<Endpoint> CreateEndpoints(IReadOnlyList<ServiceEntryDescriptor> serviceEntryDescriptors,
        IReadOnlyList<Action<EndpointBuilder>> conventions)
    {
        var endpoints = new List<Endpoint>();
        var routeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var serviceEntryDescriptor in serviceEntryDescriptors)
        {
            _serviceEntryDescriptorEndpointFactory.AddEndpoints(endpoints, routeNames, serviceEntryDescriptor,
                conventions);
        }

        return endpoints;
    }

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            Initialize();
            Debug.Assert(_changeToken != null);
            Debug.Assert(_endpoints != null);
            return _endpoints;
        }
    }
}