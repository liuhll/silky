using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Routing.Template;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class ServiceEntry
    {
        public Func<string, IDictionary<ParameterFrom, object>, Task<object>> Func { get; set; }

        public RouteTemplate RouteTemplate { get; set; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; set; }

        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}