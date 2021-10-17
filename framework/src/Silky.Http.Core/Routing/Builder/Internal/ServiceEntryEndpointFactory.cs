using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Silky.Core;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Routing.Builder.Internal
{
    internal class ServiceEntryEndpointFactory
    {
        private RequestDelegate CreateRequestDelegate(ServiceEntry serviceEntry)
        {
            return async httpContext =>
            {
                var messageReceivedHandler = EngineContext.Current.Resolve<IMessageReceivedHandler>();
                await messageReceivedHandler.Handle(serviceEntry, httpContext);
            };
        }

        public void AddEndpoints(List<Endpoint> endpoints,
            HashSet<string> routeNames,
            ServiceEntry serviceEntry,
            IReadOnlyList<Action<EndpointBuilder>> conventions)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (routeNames == null)
            {
                throw new ArgumentNullException(nameof(routeNames));
            }

            if (serviceEntry == null)
            {
                throw new ArgumentNullException(nameof(serviceEntry));
            }

            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (serviceEntry.GovernanceOptions.ProhibitExtranet)
            {
                return;
            }

            var requestDelegate = CreateRequestDelegate(serviceEntry);
            var builder =
                new RouteEndpointBuilder(requestDelegate, RoutePatternFactory.Parse(serviceEntry.Router.RoutePath),
                    serviceEntry.Router.RouteTemplate.Order)
                {
                    DisplayName = serviceEntry.Id,
                };
            builder.Metadata.Add(serviceEntry);
            builder.Metadata.Add(serviceEntry.Id);
            builder.Metadata.Add(new HttpMethodMetadata(new[] { serviceEntry.Router.HttpMethod.ToString() }));
            if (!serviceEntry.ServiceEntryDescriptor.IsAllowAnonymous && serviceEntry.AuthorizeData != null)
            {
                foreach (var data in serviceEntry.AuthorizeData)
                {
                    builder.Metadata.Add(data);
                }
            }

            endpoints.Add(builder.Build());
        }
    }
}