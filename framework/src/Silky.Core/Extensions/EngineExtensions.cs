using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Silky.Core.Extensions
{
    public static class EngineExtensions
    {
        public static bool ContainModule([NotNull] this IEngine engine, [NotNull] string moduleName)
        {
            Check.NotNull(engine, nameof(engine));
            Check.NotNull(moduleName, nameof(moduleName));
            return (engine as SilkyEngine)?.Modules.Any(p => p.Name == moduleName) == true;
        }

        public static bool IsContainHttpCoreModule([NotNull] this IEngine engine)
        {
            Check.NotNull(engine, nameof(engine));
            return engine.ContainModule("SilkyHttpCore");
        }

        public static bool IsContainWebSocketModule([NotNull] this IEngine engine)
        {
            Check.NotNull(engine, nameof(engine));
            return engine.ContainModule("WebSocket");
        }

        // public static bool IsContainDotNettyTcpModule([NotNull] this IEngine engine)
        // {
        //     Check.NotNull(engine, nameof(engine));
        //     return engine.ContainModule("DotNettyTcp");
        // }

        public static bool IsRpcServerProvider([NotNull] this IEngine engine)
        {
            Check.NotNull(engine, nameof(engine));
            var serverMessageListenerTypeName = "Silky.Rpc.Runtime.Server.IServerMessageListener,Silky.Rpc";
            var serverMessageListenerType = Type.GetType(serverMessageListenerTypeName);
            if (serverMessageListenerType == null)
            {
                return false;
            }

            return engine.IsRegistered(serverMessageListenerType);
        }

        public static bool IsEnvironment([NotNull] this IEngine engine, string environmentName)
        {
            Check.NotNull(engine, nameof(engine));
            if (engine.HostEnvironment != null)
            {
                return engine.IsEnvironment(environmentName);
            }

            var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (!dotnetEnvironment.IsNullOrEmpty())
            {
                return dotnetEnvironment.Equals(environmentName);
            }

            var aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!aspNetCoreEnvironment.IsNullOrEmpty())
            {
                return aspNetCoreEnvironment.Equals(environmentName);
            }

            return false;
        }
    }
}