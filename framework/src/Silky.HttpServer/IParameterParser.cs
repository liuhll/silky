using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Http;

namespace Silky.HttpServer
{
    internal interface IParameterParser : ITransientDependency
    {
        Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request, ServiceEntry serviceEntry);
    }
}