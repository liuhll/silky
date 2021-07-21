using System;

namespace Silky.EntityFrameworkCore.Entities.Attributes
{
    /// <summary>
    /// 手动配置实体特性
    /// </summary>
    /// <remarks>支持类和方法</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ManualAttribute : Attribute
    {
    }
}