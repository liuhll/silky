using System.Threading;

namespace Silky.Transaction
{
    public class SilkyTransactionHolder
    {
        private static readonly SilkyTransactionHolder instance = new ();

        private static readonly AsyncLocal<ITransaction> CURRENT = new();

        private SilkyTransactionHolder()
        {
        }

        public static SilkyTransactionHolder Instance => instance;

        public void Set(ITransaction transaction)
        {
            CURRENT.Value = transaction;
        }

        public ITransaction CurrentTransaction => CURRENT.Value;
       

        public void remove()
        {
            CURRENT.Value = null;
        }
    }
}