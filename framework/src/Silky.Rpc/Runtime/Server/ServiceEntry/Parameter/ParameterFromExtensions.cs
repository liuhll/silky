using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Server
{
    public static class ParameterFromExtensions
    {
        public static object DefaultValue(this ParameterFrom @form)
        {
            var defaultValue = new Dictionary<string, object>();
            return defaultValue;
        }
    }
}