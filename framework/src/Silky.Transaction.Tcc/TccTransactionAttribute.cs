using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Transaction.Tcc
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TccTransactionAttribute : Attribute, ITccTransactionProvider
    {
        public string ConfirmMethod { get; set; }

        public string CancelMethod { get; set; }
    }
}