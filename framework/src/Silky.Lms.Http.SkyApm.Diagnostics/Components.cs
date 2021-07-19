using SkyApm.Common;

namespace Silky.Lms.Http.SkyApm.Diagnostics
{
    public static class Components
    {
        public static readonly StringOrIntValue LmsHttp = new StringOrIntValue(80, "lmshttp");
    }
}