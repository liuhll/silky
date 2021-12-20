using System;

namespace Silky.Rpc.Runtime.Server;

public class ServerExceptionContext
{
    public Exception Exception { get; set; }
    public ServiceEntry ServiceEntry { get; set; }
}