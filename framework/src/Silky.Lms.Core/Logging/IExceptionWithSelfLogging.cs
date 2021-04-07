using Microsoft.Extensions.Logging;

namespace Silky.Lms.Core.Logging
{
    public interface IExceptionWithSelfLogging
    {
        void Log(ILogger logger);
    }
}