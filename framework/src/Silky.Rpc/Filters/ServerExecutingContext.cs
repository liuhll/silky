using System;
using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerExecutingContext : FilterContext
    {
        public ServerExecutingContext(ServiceEntryContext context, IList<IFilterMetadata> filters) : base(context,
            filters)
        {
            InstanceType = context.ServiceInstance.GetType();
        }

        public Type InstanceType { get; private set; }

        public virtual object Result { get; set; }
    }
}