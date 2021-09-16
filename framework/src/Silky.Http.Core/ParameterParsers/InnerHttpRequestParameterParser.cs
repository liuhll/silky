using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core
{
    public class InnerHttpRequestParameterParser : IParameterParser
    {
        public Task<object[]> Parser(ServiceEntry serviceEntry)
        {
            throw new SilkyException("rpc communication does not support http protocol temporarily");
        }
    }
}