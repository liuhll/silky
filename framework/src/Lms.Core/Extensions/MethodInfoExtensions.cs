using System;
using System.Reflection;
using Lms.Core.MethodExecutor;

namespace Lms.Core.Extensions
{
    public static class MethodInfoExtensions
    {
        public static (bool, Type) ReturnTypeInfo(this MethodInfo methodInfo)
        {
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return (isAwaitable, isAwaitable ? coercedAwaitableInfo.AwaitableInfo.ResultType : methodInfo.ReturnType);
        }

        public static Type GetReturnType(this MethodInfo methodInfo)
        {
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return isAwaitable ? coercedAwaitableInfo.AwaitableInfo.ResultType : methodInfo.ReturnType;
        }

        public static bool IsAsyncMethodInfo(this MethodInfo methodInfo)
        {
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return isAwaitable;
        }
    }
}