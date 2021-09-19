using System;

namespace Silky.Core.DbContext.UnitOfWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnitOfWorkAttribute : Attribute
    {
        public UnitOfWorkAttribute()
        {
        }

        public UnitOfWorkAttribute(bool ensureTransaction)
        {
            EnsureTransaction = ensureTransaction;
        }

        public bool EnsureTransaction { get; set; } = true;
    }
}