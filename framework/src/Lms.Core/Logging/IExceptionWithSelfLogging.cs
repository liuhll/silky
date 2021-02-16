using Microsoft.Extensions.Logging;

namespace Lms.Core.Logging
{
    public interface IExceptionWithSelfLogging
    {
        void Log(ILogger logger);
    }
}