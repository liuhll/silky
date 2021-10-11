using SkyApm.Common;

namespace Silky.SkyApm.Diagnostics.Abstraction
{
    public class SilkyComponents
    {
        public static readonly StringOrIntValue SilkyRpc = new(2001, "SilkyRpc");

        public static StringOrIntValue SilkyTransaction = new(2002, "SilkyTransaction");
    }
}