using System.Threading.Tasks;

namespace Silky.Rpc.Address
{
    public delegate Task HealthChangeEvent(IRpcAddress rpcAddress, bool isHealth);

    public delegate Task UnhealthEvent(IRpcAddress rpcAddress);

    public delegate Task RemoveAddressEvent(IRpcAddress rpcAddress);

    public delegate Task AddMonitorEvent(IRpcAddress rpcAddress);
    
}