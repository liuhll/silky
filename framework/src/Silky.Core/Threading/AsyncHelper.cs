using System;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nito.AsyncEx;

namespace Silky.Core.Threading;

public static class AsyncHelper
{
    public static bool IsAsync([NotNull] this MethodInfo method)
    {
        Check.NotNull(method, nameof(method));

        return method.ReturnType.IsTaskOrTaskOfT();
    }

    public static bool IsTaskOrTaskOfT([NotNull] this Type type)
    {
        return type == typeof(Task) ||
               (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }

    public static bool IsTaskOfT([NotNull] this Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
    }

    public static Type UnwrapTask([NotNull] Type type)
    {
        Check.NotNull(type, nameof(type));

        if (type == typeof(Task))
        {
            return typeof(void);
        }

        if (type.IsTaskOfT())
        {
            return type.GenericTypeArguments[0];
        }

        return type;
    }

    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        return AsyncContext.Run(func);
    }

    public static void RunSync(Func<Task> action)
    {
        AsyncContext.Run(action);
    }
}