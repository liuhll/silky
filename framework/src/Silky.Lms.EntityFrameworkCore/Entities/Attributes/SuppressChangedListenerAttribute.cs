
using System;

namespace Silky.Lms.EntityFrameworkCore.Entities.Attributes
{
    /// <summary>
    /// 禁止实体监听
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SuppressChangedListenerAttribute : Attribute
    {
    }
}