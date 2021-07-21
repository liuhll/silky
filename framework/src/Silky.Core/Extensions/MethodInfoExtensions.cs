using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core.MethodExecutor;

namespace Silky.Core.Extensions
{
    public static class MethodInfoExtensions
    {
        public static (bool, Type) ReturnTypeInfo([NotNull] this MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return (isAwaitable, isAwaitable ? coercedAwaitableInfo.AwaitableInfo.ResultType : methodInfo.ReturnType);
        }

        public static Type GetReturnType(this MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return isAwaitable ? coercedAwaitableInfo.AwaitableInfo.ResultType : methodInfo.ReturnType;
        }

        public static bool IsAsyncMethodInfo([NotNull] this MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return isAwaitable;
        }

        public static bool AchievingEquality([NotNull] this MethodInfo methodInfo, MethodInfo another)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            if (another == null)
            {
                return false;
            }

            if (methodInfo.Name != another.Name)
            {
                return false;
            }

            if (methodInfo.GetParameters().Length != another.GetParameters().Length)
            {
                return false;
            }

            var parameters = methodInfo.GetParameters();
            var anotherParameters = another.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != anotherParameters[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ParameterEquality([NotNull] this MethodInfo methodInfo, [NotNull] MethodInfo another)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            Check.NotNull(another, nameof(another));

            if (methodInfo.GetParameters().Length != another.GetParameters().Length)
            {
                return false;
            }

            var parameters = methodInfo.GetParameters();
            var anotherParameters = another.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != anotherParameters[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        public static ObjectMethodExecutor CreateExecutor([NotNull] this MethodInfo methodInfo, [NotNull] Type type)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            Check.NotNull(type, nameof(type));
            var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(methodInfo);
            return ObjectMethodExecutor.Create(methodInfo, type.GetTypeInfo(), parameterDefaultValues);
        }
    }
}