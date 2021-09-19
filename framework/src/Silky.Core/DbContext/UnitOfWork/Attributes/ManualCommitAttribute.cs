using System;

namespace Silky.Core.DbContext.UnitOfWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ManualCommitAttribute : Attribute
    {
    }
}