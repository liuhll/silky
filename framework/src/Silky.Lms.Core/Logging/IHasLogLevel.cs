using Microsoft.Extensions.Logging;

namespace Silky.Lms.Core.Logging
{
    public interface IHasLogLevel
    {
        LogLevel LogLevel { get; set; }
    }
}