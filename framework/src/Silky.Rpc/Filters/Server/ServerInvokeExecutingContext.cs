using System;
using System.Collections.Generic;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerInvokeExecutingContext : ServerFilterContext
    {
        public ServerInvokeExecutingContext(ServiceEntryContext context, IList<IFilterMetadata> filters,
            IDictionary<string, object> serviceEntryArguments) : base(context,
            filters)
        {
            InstanceType = context.ServiceInstance.GetType();
            ServiceEntryArguments = serviceEntryArguments;
        }

        public Type InstanceType { get; private set; }

        public virtual object Result { get; set; }
        
        public virtual IDictionary<string, object> ServiceEntryArguments { get; }
    }
}