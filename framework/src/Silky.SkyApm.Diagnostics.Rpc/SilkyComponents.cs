using SkyApm.Common;

namespace Silky.SkyApm.Diagnostics.Rpc
{
    public class SilkyComponents
    {
        public static readonly StringOrIntValue SilkyRpc = new(2001, "SilkyRpc");
        
        public static StringOrIntValue SilkyStartTransaction = new(2002, "SilkyStartTransaction");
        public static StringOrIntValue SilkyParticipantTransaction = new(2003, "SilkyParticipantTransaction");
    }
}