using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Http;

namespace Lms.HttpServer
{ 
    internal interface IParameterParser : ITransientDependency
    {
        Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request, ServiceEntry serviceEntry);
    }
}