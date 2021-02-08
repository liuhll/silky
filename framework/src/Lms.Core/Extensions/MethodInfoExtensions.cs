using System;
using System.Reflection;
using Lms.Core.MethodExecutor;

namespace Lms.Core.Extensions
{
    public static class MethodInfoExtensions
    {
        public static (bool, Type) MethodInfoReturnType(this MethodInfo methodInfo)
        {
            var isAwaitable = CoercedAwaitableInfo.IsTypeAwaitable(methodInfo.ReturnType, out var coercedAwaitableInfo);
            return (isAwaitable, isAwaitable ? coercedAwaitableInfo.AwaitableInfo.ResultType : methodInfo.ReturnType);
        }
    }
}