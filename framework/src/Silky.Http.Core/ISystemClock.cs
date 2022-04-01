using System;

namespace Silky.Http.Core;

internal interface ISystemClock
{
    DateTime UtcNow { get; }
}