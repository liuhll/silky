using Silky.Core;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions;

public static class EngineExtensions
{
    public static bool IsGateway(this IEngine engine)
    {
        return !engine.IsRegistered(typeof(IServerMessageListener));
    }
}