using System.Threading.Tasks;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public delegate Task OnRemoveRpcEndpoint(string hostName, ISilkyEndpoint silkyEndpoint);

    public delegate Task OnUpdateRpcEndpoint(string hostName,ISilkyEndpoint[] rpcEndpoints);
}