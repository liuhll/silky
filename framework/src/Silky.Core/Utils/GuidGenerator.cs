using System;

namespace Silky.Core.Utils
{
    public static class GuidGenerator
    {
        public static Guid Create()
        {
            return Guid.NewGuid();
        }

        public static string CreateGuidStr()
        {
            return Create().ToString();
        }

        public static string CreateGuidStrWithNoUnderline()
        {
            return Create().ToString("N");
        }
    }
}