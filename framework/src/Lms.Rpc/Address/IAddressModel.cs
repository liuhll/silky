using System.Net;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Address
{
    public interface IAddressModel
    {
        /// <summary>
        /// 地址(Ip或是域名)
        /// </summary>
        string Address { get; }
        
        /// <summary>
        /// 指定的端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 地址类型
        /// </summary>
        ServiceProtocol ServiceProtocol { get; }
        
        event HealthChange HealthChange;

        void ChangeHealthState(bool isHealth);

        /// <summary>
        /// ip终结点
        /// </summary>
        IPEndPoint IPEndPoint { get; }

        /// <summary>
        /// 地址描述符
        /// </summary>
        AddressDescriptor Descriptor { get; }
    }
}