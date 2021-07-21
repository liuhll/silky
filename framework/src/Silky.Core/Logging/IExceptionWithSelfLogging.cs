using Microsoft.Extensions.Logging;

namespace Silky.Core.Logging
{
    public interface IExceptionWithSelfLogging
    {
        void Log(ILogger logger);
    }
}