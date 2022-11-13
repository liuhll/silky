using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Silky.Http.Core.Routing.Builder.Internal;

internal abstract class SilkyEndpointDataSourceBase : EndpointDataSource
{
    public SilkyRpcServiceEndpointConventionBuilder DefaultBuilder { get; protected set; }

    protected List<Endpoint>? _endpoints;

    protected IChangeToken? _changeToken;

    protected readonly object Lock = new();


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

    protected abstract void UpdateEndpoints();
    
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