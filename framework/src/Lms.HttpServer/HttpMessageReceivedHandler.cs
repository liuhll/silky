using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DependencyInjection;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.HttpServer
{
    internal class HttpMessageReceivedHandler : ITransientDependency
    {
        private readonly IServiceEntryLocate _serviceEntryLocate;

        public HttpMessageReceivedHandler(IServiceEntryLocate serviceEntryLocate)
        {
            _serviceEntryLocate = serviceEntryLocate;
        }

        internal async Task Handle(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = _serviceEntryLocate.GetServiceEntryByApi(path, method);

            if (serviceEntry.IsLocal)
            {
                var streamReader = new StreamReader(context.Request.Body);
                var requestData = await streamReader.ReadToEndAsync();
                var responseData =  await serviceEntry.Executor(null,new Dictionary<ParameterFrom, object>()
                {
                    {ParameterFrom.Body,requestData}
                });

                await context.Response.WriteAsync(EngineContext.Current.Resolve<IJsonSerializer>().Serialize(responseData));
            }

        }
    }
}