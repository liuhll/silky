using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;

namespace Lms.Transaction.Tcc
{
    public static class ServiceEntryExtensions
    {
        public static bool IsTransactionServiceEntry([NotNull]this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            return serviceEntry.CustomAttributes.OfType<TransactionAttribute>().Any();
        }
        
        public static ITccTransactionProvider GetTccTransactionProvider([NotNull]this ServiceEntry serviceEntry,
            string serviceKey)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Debug.Assert(serviceEntry.IsLocal);
            var instance =
                EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            var methods = instance.GetType().GetTypeInfo().GetMethods();
            
            var implementationMethod = methods.Single(p => p.AchievingEquality(serviceEntry.MethodInfo));

            return implementationMethod.GetCustomAttributes().OfType<ITccTransactionProvider>().FirstOrDefault();
        }
        
    }
}