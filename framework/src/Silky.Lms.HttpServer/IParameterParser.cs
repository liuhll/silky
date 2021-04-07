using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Http;

namespace Silky.Lms.HttpServer
{
    internal interface IParameterParser : ITransientDependency
    {
        Task<IDictionary<ParameterFrom, object>> Parser(HttpRequest request, ServiceEntry serviceEntry);
    }
}