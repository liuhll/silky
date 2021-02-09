using System;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.HttpServer
{
    internal class HttpMessageReceivedHandler : ITransientDependency
    {
        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly IParameterParser _parameterParser;
        private readonly IServiceExecutor _serverExecutor;
        private readonly ISerializer _serializer;

        public HttpMessageReceivedHandler(IServiceEntryLocate serviceEntryLocate,
            IParameterParser parameterParser,
            IServiceExecutor serverExecutor,
            ISerializer serializer)
        {
            _serviceEntryLocate = serviceEntryLocate;
            _parameterParser = parameterParser;
            _serverExecutor = serverExecutor;
            _serializer = serializer;
        }

        internal async Task Handle(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = _serviceEntryLocate.GetServiceEntryByApi(path, method);
            if (serviceEntry == null)
            {
                throw new LmsException($"通过{path}-{method}无法找到服务条目");
            }
            var parameters = await _parameterParser.Parser(context.Request, serviceEntry);
            var excuteResult = await _serverExecutor.Execute(serviceEntry, parameters);
            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = 200;
            if (excuteResult != null)
            {
                var responseData = _serializer.Serialize(excuteResult);
                context.Response.ContentLength = responseData.GetBytes().Length;
                await context.Response.WriteAsync(responseData);
            }
            else
            {
                context.Response.ContentLength = 0;
                await context.Response.WriteAsync(null);
            }
        }
    }
}