using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Runtime.Server
{
    public enum ServiceProtocol
    {
        Tcp,
        Mqtt,
        Ws,
    }
}