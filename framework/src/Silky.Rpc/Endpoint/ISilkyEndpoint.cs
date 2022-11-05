using System;
using System.Net;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Endpoint
{
    public interface ISilkyEndpoint
    {
        /// <summary>
        /// 地址(Ip或是域名)
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 指定的端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 地址类型
        /// </summary>
        ServiceProtocol ServiceProtocol { get; }

        /// <summary>
        /// ip终结点
        /// </summary>
        IPEndPoint IPEndPoint { get; }

        /// <summary>
        ///  该地址是否可用
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// 上次不可用时间
        /// </summary>
        DateTime? LastDisableTime { get; }

        /// <summary>
        /// 让地址熔断
        /// </summary>
        void MakeFusing(int fuseSleepDuration);

        void InitFuseTimes();

        int FuseTimes { get; }

        /// <summary>
        /// 地址描述符
        /// </summary>
        SilkyEndpointDescriptor Descriptor { get; }
    }
}