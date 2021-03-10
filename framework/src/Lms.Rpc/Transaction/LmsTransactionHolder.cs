using System.Threading;

namespace Lms.Rpc.Transaction
{
    public class LmsTransactionHolder
    {
        private static LmsTransactionHolder instance = new LmsTransactionHolder();

        private static AsyncLocal<ITransaction> CURRENT = new();

        private LmsTransactionHolder()
        {
        }

        public static LmsTransactionHolder Instance => instance;

        public void Set(ITransaction transaction)
        {
            CURRENT.Value = transaction;
        }

        public void remove()
        {
            CURRENT.Value = null;
        }
    }
}