using Microsoft.Extensions.Logging;

namespace Lms.Core.Logging
{
    public interface IHasLogLevel
    {
        LogLevel LogLevel { get; set; }
    }
}