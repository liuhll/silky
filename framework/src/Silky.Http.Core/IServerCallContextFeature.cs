namespace Silky.Http.Core;

public abstract class IServerCallContextFeature
{
    HttpContextServerCallContext ServerCallContext { get; }
}