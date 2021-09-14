namespace Silky.Rpc.Runtime.Client
{
    public class ServiceEntryInvokeInfo
    {
        public ServiceInvokeInfo ServiceInvokeInfo { get; set; }

        public string Address { get; set; }

        public bool IsEnable { get; set; }

        public string ServiceEntryId { get; set; }
    }
}