using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Rpc;

namespace Silky.Http.Core
{
    internal class DefaultHttpRequestParameterParser : IParameterParser
    {
        private readonly ISerializer _serializer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultHttpRequestParameterParser(ISerializer serializer,
            IHttpContextAccessor httpContextAccessor)
        {
            _serializer = serializer;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request, ServiceEntry serviceEntry)
        {
            var parameters = new Dictionary<ParameterFrom, object>();
            if (request.HasFormContentType)
            {
                var formData = request.Form.ToDictionary(p => p.Key, p => p.Value.ToString());
                parameters.Add(ParameterFrom.Form, _serializer.Serialize(formData));
            }

            if (request.Query.Any())
            {
                var queryData = request.Query.ToDictionary(p => p.Key, p => p.Value.ToString());
                parameters.Add(ParameterFrom.Query, _serializer.Serialize(queryData));
            }

            if (request.Headers.Any())
            {
                var headerData = request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString());
                RpcContext.Context.SetAttachment(AttachmentKeys.RequestHeader, headerData);
                parameters.Add(ParameterFrom.Header, _serializer.Serialize(headerData));
            }

            if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var streamReader = new StreamReader(request.Body);
                var bodyData = await streamReader.ReadToEndAsync();
                parameters.Add(ParameterFrom.Body, bodyData);
            }

            if (serviceEntry != null && serviceEntry.ParameterDescriptors.Any(p => p.From == ParameterFrom.Path))
            {
                var pathData = serviceEntry.Router.ParserRouteParameters(request.Path);
                parameters.Add(ParameterFrom.Path, _serializer.Serialize(pathData));
            }

            return parameters;
        }

        public async Task<object[]> Parser([NotNull] ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(_httpContextAccessor.HttpContext, nameof(_httpContextAccessor.HttpContext.Request));
            var requestParameters = await Parser(_httpContextAccessor.HttpContext.Request, serviceEntry);
            return serviceEntry.ResolveParameters(requestParameters);
        }
    }
}