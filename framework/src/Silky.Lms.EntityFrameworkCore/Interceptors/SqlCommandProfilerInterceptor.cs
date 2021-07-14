﻿using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Silky.Lms.EntityFrameworkCore.Interceptors
{
    /// <summary>
    /// 数据库执行命令拦截
    /// </summary>
    internal sealed class SqlCommandProfilerInterceptor : DbCommandInterceptor
    {
    }
}