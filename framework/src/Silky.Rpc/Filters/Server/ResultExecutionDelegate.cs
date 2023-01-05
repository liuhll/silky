using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public delegate Task<ServerResultExecutedContext> ResultExecutionDelegate();