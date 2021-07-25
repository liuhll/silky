using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Filters
{
    public class ServiceEntryExecutingContext
    {
        public ServiceEntry ServiceEntry { get; set; }

        public object[] Parameters { get; set; }

        public string ServiceKey { get; set; }
    }
}