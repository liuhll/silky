using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Transaction.Tcc
{
    public static class ServiceEntryExtensions
    {
        public static ITccTransactionProvider GetTccTransactionProvider([NotNull] this ServiceEntry serviceEntry,
            string serviceKey)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            if (!serviceEntry.IsLocal)
            {
                return null;
            }

            var instance =
                EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            var methods = instance.GetType().GetTypeInfo().GetMethods();

            var implementationMethod = methods.Single(p => p.AchievingEquality(serviceEntry.MethodInfo));

            return implementationMethod.GetCustomAttributes().OfType<ITccTransactionProvider>().FirstOrDefault();
        }

        public static (ObjectMethodExecutor, bool, object) GetTccExcutorInfo([NotNull] this ServiceEntry serviceEntry,
            string serviceKey, TccMethodType tccMethodType)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Debug.Assert(serviceEntry.IsLocal);
            var instance =
                EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            var methods = instance.GetType().GetTypeInfo().GetMethods();
            var implementationMethod = methods.Single(p => p.AchievingEquality(serviceEntry.MethodInfo));
            if (tccMethodType == TccMethodType.Try)
            {
                return (implementationMethod.CreateExecutor(instance.GetType()),
                    implementationMethod.IsAsyncMethodInfo(), instance);
            }

            var tccTransactionProvider =
                implementationMethod.GetCustomAttributes().OfType<ITccTransactionProvider>().First();
            MethodInfo execMethod;
            if (tccMethodType == TccMethodType.Confirm)
            {
                execMethod = GetCompareMethod(methods, implementationMethod, tccTransactionProvider.ConfirmMethod);
            }
            else
            {
                execMethod = GetCompareMethod(methods, implementationMethod, tccTransactionProvider.CancelMethod);
            }

            if (execMethod == null)
            {
                throw new SilkyException($"未定义{tccMethodType}方法");
            }

            return (execMethod.CreateExecutor(instance.GetType()), implementationMethod.IsAsyncMethodInfo(),
                instance);
        }

        private static MethodInfo GetCompareMethod(MethodInfo[] methodInfos, MethodInfo tryMethod,
            string compareMethodName)
        {
            var compareMethods = methodInfos.Where(p => p.Name == compareMethodName);
            MethodInfo compareMethod = null;
            foreach (var methodInfo in compareMethods)
            {
                if (methodInfo.ParameterEquality(tryMethod))
                {
                    compareMethod = methodInfo;
                    break;
                }
            }

            return compareMethod;
        }
    }
}