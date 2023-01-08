using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public delegate Task<ClientInvokeExecutedContext> ClientInvokeExecutionDelegate();