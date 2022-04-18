using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Silky.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Routing.Builder.Internal;

public class ServiceEntryDescriptorEndpointFactory
{
    private RequestDelegate CreateRequestDelegate(ServiceEntryDescriptor serviceEntryDescriptor)
    {
        return async httpContext =>
        {
            var messageReceivedHandler = EngineContext.Current.Resolve<IMessageReceivedHandler>();
            await messageReceivedHandler.Handle(serviceEntryDescriptor, httpContext);
        };
    }

    public void AddEndpoints(List<Endpoint> endpoints, HashSet<string> routeNames,
        ServiceEntryDescriptor serviceEntryDescriptor, IReadOnlyList<Action<EndpointBuilder>> conventions)
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        if (routeNames == null)
        {
            throw new ArgumentNullException(nameof(routeNames));
        }

        if (serviceEntryDescriptor == null)
        {
            throw new ArgumentNullException(nameof(serviceEntryDescriptor));
        }

        if (conventions == null)
        {
            throw new ArgumentNullException(nameof(conventions));
        }

        if (serviceEntryDescriptor.GovernanceOptions.ProhibitExtranet)
        {
            return;
        }

        var requestDelegate = CreateRequestDelegate(serviceEntryDescriptor);
        var builder =
            new RouteEndpointBuilder(requestDelegate, RoutePatternFactory.Parse(serviceEntryDescriptor.WebApi),
                serviceEntryDescriptor.RouteOrder)

            {
                DisplayName = serviceEntryDescriptor.Id,
            };
        builder.Metadata.Add(serviceEntryDescriptor);
        builder.Metadata.Add(serviceEntryDescriptor.Id);
        builder.Metadata.Add(new HttpMethodMetadata(new[] { serviceEntryDescriptor.HttpMethod.ToString() }));
        if (!serviceEntryDescriptor.IsAllowAnonymous && serviceEntryDescriptor.AuthorizeData != null)
        {
            var gatewayOptions = EngineContext.Current.GetOptions<GatewayOptions>();
            if (serviceEntryDescriptor.AuthorizeData.Any())
            {
                foreach (var data in serviceEntryDescriptor.AuthorizeData)
                {
                    var authorizeAttribute = new AuthorizeAttribute()
                    {
                        AuthenticationSchemes = data.AuthenticationSchemes,
                        Policy = data.Policy,
                        Roles = data.Roles
                    };
                    builder.Metadata.Add(authorizeAttribute);
                }
            }
            else if (gatewayOptions.GlobalAuthorize)
            {
                builder.Metadata.Add(new AuthorizeAttribute());
            }
        }

        endpoints.Add(builder.Build());
    }
}