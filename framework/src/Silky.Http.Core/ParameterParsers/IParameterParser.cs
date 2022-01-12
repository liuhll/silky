using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core
{
    internal interface IParameterParser : ITransientDependency
    {
        Task<object[]> Parser([NotNull] HttpRequest httpRequest,[NotNull] ServiceEntry serviceEntry);
        
    }
}