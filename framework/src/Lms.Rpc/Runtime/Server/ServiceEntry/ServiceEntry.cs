using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class ServiceEntry
    {
        public Func<string, IDictionary<string, object>, Task<object>> Func { get; set; }

        public ServiceDescriptor Descriptor { get; set; }
    }
}