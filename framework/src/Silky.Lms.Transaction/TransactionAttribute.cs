using System;

namespace Silky.Lms.Transaction
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TransactionAttribute : Attribute
    {
        
    }
}