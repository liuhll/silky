namespace Silky.Rpc.Runtime.Server.Filters
{
    public class ServiceEntryExecutingContext
    {
        public ServiceEntry ServiceEntry { get; set; }

        public object[] Parameters { get; set; }

        public string ServiceKey { get; set; }
    }
}