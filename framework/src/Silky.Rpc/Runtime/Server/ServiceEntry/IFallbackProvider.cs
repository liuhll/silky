using System;

namespace Silky.Rpc.Runtime.Server
{
    public interface IFallbackProvider
    {
        Type Type { get; }

        string MethodName { get; set; }

        int Weight { get; set; }
    }
}