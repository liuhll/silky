using System.Reflection;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSampleType(this ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType.IsValueType || parameterInfo.ParameterType.IsEnum || parameterInfo.ParameterType == typeof(string))
            {
                return true;
            }

            return false;
        }
    }
}