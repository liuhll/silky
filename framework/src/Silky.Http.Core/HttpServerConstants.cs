namespace Silky.Http.Core;

internal static class HttpServerConstants
{
    internal const string HostActivityName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";
    internal const string HostActivityChanged = HostActivityName + ".Changed";

    internal const string ActivityStatusCodeTag = "silkyrpc.status_code";
    internal const string ActivityMethodTag = "silkyrpc.method";
}