using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Routing
{
    public interface IRoutePathParser : ITransientDependency
    {
        string Parse(string routeTemplate, string serviceName, string methodName, bool routeIsReWriteByServiceRoute);
    }
}