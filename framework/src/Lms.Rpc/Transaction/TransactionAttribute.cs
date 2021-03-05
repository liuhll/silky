using System;

namespace Lms.Rpc.Transaction
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TransactionAttribute : Attribute
    {
        
    }
}