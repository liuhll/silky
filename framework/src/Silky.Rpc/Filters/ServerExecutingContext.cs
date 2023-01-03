using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerExecutingContext : ServiceEntryContext
    {
        public ServerExecutingContext(ServiceEntryContext context) : base(context)
        {
            InstanceType = context.ServiceInstance.GetType();
        }

        public Type InstanceType { get; private set; }
    }
}