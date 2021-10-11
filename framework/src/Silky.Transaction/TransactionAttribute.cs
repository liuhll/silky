using System;

namespace Silky.Transaction
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TransactionAttribute : Attribute
    {
    }
}