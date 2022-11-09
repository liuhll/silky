using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Silky.Core.Logging;

public interface IInitLogger<out T> : ILogger<T>
{
    public List<SilkyInitLogEntry> Entries { get; }
}