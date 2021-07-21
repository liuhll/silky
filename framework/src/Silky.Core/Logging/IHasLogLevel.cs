using Microsoft.Extensions.Logging;

namespace Silky.Core.Logging
{
    public interface IHasLogLevel
    {
        LogLevel LogLevel { get; set; }
    }
}