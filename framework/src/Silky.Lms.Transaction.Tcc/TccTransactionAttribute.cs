using System;

namespace Silky.Lms.Transaction.Tcc
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TccTransactionAttribute : Attribute, ITccTransactionProvider
    {
        public string ConfirmMethod { get; set; }

        public string CancelMethod { get; set; }
    }
}