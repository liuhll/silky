using System.Reflection;

namespace Silky.Lms.Validation
{
    public class MethodInvocationValidationContext : LmsValidationResult
    {
        public MethodInfo Method { get; }

        public object[] ParameterValues { get; }

        public ParameterInfo[] Parameters { get; }

        public MethodInvocationValidationContext(MethodInfo method, object[] parameterValues)
        {
            Method = method;
            ParameterValues = parameterValues;
            Parameters = method.GetParameters();
        }
    }
}