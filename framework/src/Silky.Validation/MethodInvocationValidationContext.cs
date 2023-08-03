using System.Reflection;

namespace Silky.Validation
{
    public class MethodInvocationValidationContext : SilkyValidationResult
    {
        public MethodInfo Method { get; }

        public object[] ParameterValues { get; }

        public ParameterInfo[] Parameters { get; }

        public ValidationType ValidationType { get; set; }

        public MethodInvocationValidationContext(MethodInfo method, 
            object[] parameterValues, 
            ValidationType validationType)
        {
            Method = method;
            ParameterValues = parameterValues;
            ValidationType = validationType;
            Parameters = method.GetParameters();
        }
    }
}