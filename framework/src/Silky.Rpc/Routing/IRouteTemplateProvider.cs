using System;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Routing
{
    public interface IRouteTemplateProvider
    {
        string Template { get; }

        string ServiceName { get; set; }
    }
}