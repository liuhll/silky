using System;
using Newtonsoft.Json.Linq;

namespace Silky.Codec
{
    public static class UtilityType
    {
        public static Type JObjectType = typeof(JObject);

        public static Type JArrayType = typeof(JArray);

        public static Type ObjectType = typeof(Object);

        public static Type ConvertibleType = typeof(IConvertible);
    }
}