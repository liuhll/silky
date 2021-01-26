using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Routing.Template
{
    public interface IRouteTemplateParser : ITransientDependency
    {
        RouteTemplate Parse(string routeTemplate, string serviceName, string methodName, bool routeIsReWriteByServiceRoute);
    }
}