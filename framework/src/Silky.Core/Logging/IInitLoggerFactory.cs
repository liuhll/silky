namespace Silky.Core.Logging;

public interface IInitLoggerFactory
{
    IInitLogger<T> Create<T>();
}