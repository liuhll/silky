using System.Reflection;
using Lms.Core.Extensions;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSampleType(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.IsSample();
        }
    }
}