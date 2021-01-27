using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class ServiceEntry
    {
        public Func<string, IDictionary<ParameterFrom, object>, Task<object>> Func { get; set; }

        public bool IsLocal { get; set; }

        public IRouter Router { get; set; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; set; }

        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}