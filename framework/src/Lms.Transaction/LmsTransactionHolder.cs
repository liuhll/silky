using System.Threading;

namespace Lms.Transaction
{
    public class LmsTransactionHolder
    {
        private static LmsTransactionHolder instance = new ();

        private static AsyncLocal<ITransaction> CURRENT = new();

        private LmsTransactionHolder()
        {
        }

        public static LmsTransactionHolder Instance => instance;

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