using System;

namespace Lms.Rpc.Transaction
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TccTransactionAttribute : Attribute
    {
        public string ConfirmMethod { get; set; }

        public string CancelMethod { get; set; }
    }
}