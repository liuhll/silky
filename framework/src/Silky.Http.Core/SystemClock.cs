using System;

namespace Silky.Http.Core;

internal class SystemClock : ISystemClock
{
    public static readonly SystemClock Instance = new SystemClock();

    public DateTime UtcNow => DateTime.UtcNow;
}