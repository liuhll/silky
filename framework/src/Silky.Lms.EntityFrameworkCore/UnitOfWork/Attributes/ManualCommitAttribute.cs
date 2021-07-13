using System;

namespace Silky.Lms.EntityFrameworkCore.UnitOfWork
{
    /// <summary>
    /// 手动提交 SaveChanges
    /// <para>默认情况下，框架会自动在方法结束之时调用 SaveChanges 方法，贴此特性可以忽略该行为</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ManualCommitAttribute : Attribute
    {
    }
}