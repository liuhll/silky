using System.Threading;
using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction
{
    public class SilkyTransactionContextHolder
    {
        private static ISilkyContext silkyContext;

        static SilkyTransactionContextHolder()
        {
            silkyContext = new ThreadLocalSilkyContext();
        }


        public static void Set(TransactionContext context)
        {
            silkyContext.Set(context);
        }

        public static TransactionContext Get()
        {
            return silkyContext.Get();
        }


        public static void Remove()
        {
            silkyContext.Remove();
        }
    }
}