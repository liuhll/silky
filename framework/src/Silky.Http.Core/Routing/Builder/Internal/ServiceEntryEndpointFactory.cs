using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Rpc;
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
                var path = httpContext.Request.Path;
                var method = httpContext.Request.Method.ToEnum<HttpMethod>();
                var logger = EngineContext.Current.Resolve<ILogger<ServiceEntryEndpointFactory>>();
                logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceEntry,
                    $"Find the ServiceEntry {serviceEntry.Id} through {path}-{method}");
                RpcContext.Context.SetAttachment(AttachmentKeys.Path, path.ToString());
                RpcContext.Context.SetAttachment(AttachmentKeys.HttpMethod, method.ToString());
                await EngineContext.Current
                    .ResolveNamed<IMessageReceivedHandler>(HttpMessageType.Outer.ToString())
                    .Handle(serviceEntry);
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
            endpoints.Add(builder.Build());
        }
    }
}