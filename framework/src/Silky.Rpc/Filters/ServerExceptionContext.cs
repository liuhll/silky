using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ServerExceptionContext
{
    public Exception Exception { get; set; }
    public ServiceEntry ServiceEntry { get; set; }
}