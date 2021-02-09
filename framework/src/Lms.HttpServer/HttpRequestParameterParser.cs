using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Serialization;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Http;

namespace Lms.HttpServer
{
    public class HttpRequestParameterParser : IParameterParser
    {
        private readonly ISerializer _serializer;

        public HttpRequestParameterParser(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request, ServiceEntry serviceEntry)
        {
            var parameters = new Dictionary<ParameterFrom, object>();
            if (request.HasFormContentType)
            {
                var formData = request.Form.ToDictionary(p => p.Key, p => p.Value.ToString());
                parameters.Add(ParameterFrom.Form,_serializer.Serialize(formData));
            }

            if (request.Query.Any())
            {
                var queryData = request.Query.ToDictionary(p => p.Key, p => p.Value.ToString());
                parameters.Add(ParameterFrom.Query,_serializer.Serialize(queryData));
            }

            if (request.Headers.Any())
            {
                var headerData = request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString());
                parameters.Add(ParameterFrom.Header,_serializer.Serialize(headerData));
            }

            if (!request.Method.Equals("GET",StringComparison.OrdinalIgnoreCase))
            {
                var streamReader = new StreamReader(request.Body);
                var bodyData = await streamReader.ReadToEndAsync();
                parameters.Add(ParameterFrom.Body,bodyData);
            }

            if (serviceEntry.ParameterDescriptors.Any(p=> p.From == ParameterFrom.Path))
            {
                var pathData = serviceEntry.Router.ParserRouteParameters(request.Path);
                parameters.Add(ParameterFrom.Path,_serializer.Serialize(pathData));
            }
            
            return parameters;
        }
    }
}