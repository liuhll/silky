using SkyApm.Common;

namespace Silky.Rpc.SkyApm.Diagnostics
{
    public static class Components
    {
        public static readonly StringOrIntValue SilkyRpc = new StringOrIntValue(2001, "SilkyRpc");

        public static readonly StringOrIntValue SilkyHttp = new StringOrIntValue(3001, "SilkyHttp");
    }
}