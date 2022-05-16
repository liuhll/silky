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
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Transport.Messages;

namespace Silky.Http.Core
{
    internal class DefaultHttpRequestParameterParser : IParameterParser
    {
        private readonly ISerializer _serializer;

        public DefaultHttpRequestParameterParser(ISerializer serializer)
        {
            _serializer = serializer;
        }

        private async Task<IDictionary<ParameterFrom, object>> ParserHttpRequest(HttpRequest request,
            ServiceEntry serviceEntry)
        {
            var parameters = new Dictionary<ParameterFrom, object>();
            if (request.HasFormContentType)
            {
                var formValueProvider = new FormValueProvider(serviceEntry, request.Form);
                var formData = formValueProvider.GetFormData();
                parameters.Add(ParameterFrom.Form, _serializer.Serialize(formData));
            }

            if (request.Query.Any())
            {
                var queryValueProvider = new QueryStringValueProvider(serviceEntry, request.Query);
                var queryData = queryValueProvider.GetQueryData();
                parameters.Add(ParameterFrom.Query, _serializer.Serialize(queryData));
            }

            if (request.Headers.Any())
            {
                var headerData = request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString());
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RequestHeader, headerData);
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

        public async Task<object[]> Parser([NotNull] HttpRequest httpRequest, [NotNull] ServiceEntry serviceEntry)
        {
            var requestParameters = await ParserHttpRequest(httpRequest, serviceEntry);
            var httpParameterResolver =
                EngineContext.Current.ResolveNamed<IParameterResolver>(ParameterType.Http.ToString());
            return httpParameterResolver.Resolve(serviceEntry, requestParameters, httpRequest.HttpContext);
        }

        public async Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request,
            ServiceEntryDescriptor serviceEntryDescriptor)
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
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RequestHeader, headerData);
                parameters.Add(ParameterFrom.Header, _serializer.Serialize(headerData));
            }

            if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var streamReader = new StreamReader(request.Body);
                var bodyData = await streamReader.ReadToEndAsync();
                parameters.Add(ParameterFrom.Body, bodyData);
            }

            return parameters;
        }
    }
}