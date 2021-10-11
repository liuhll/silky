using System;
using System.Net;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Endpoint
{
    public class RpcEndpoint : IRpcEndpoint
    {
        private int m_fuseTimes;

        public RpcEndpoint(
            [NotNull] string host,
            [NotNull] int port,
            ServiceProtocol serviceProtocol = ServiceProtocol.Tcp
        )
        {
            Check.NotNull(host, nameof(host));
            Host = host;
            Port = port;
            ServiceProtocol = serviceProtocol;
            m_fuseTimes = 0;
            Descriptor = new RpcEndpointDescriptor()
                { Host = Host, Port = Port, ServiceProtocol = ServiceProtocol };
        }

        public string Host { get; }

        public int Port { get; }

        public ServiceProtocol ServiceProtocol { get; }

        public IPEndPoint IPEndPoint => new(IPAddress.Parse(RpcEndpointHelper.GetIp(Host)), Port);

        public bool Enabled
        {
            get
            {
                if (!LastDisableTime.HasValue)
                    return true;

                return DateTime.Now > LastDisableTime.Value;
            }
        }

        public DateTime? LastDisableTime { get; private set; }

        public void MakeFusing(int fuseSleepDuration)
        {
            m_fuseTimes++;
            LastDisableTime = DateTime.Now.AddSeconds(fuseSleepDuration);
        }

        public void InitFuseTimes()
        {
            LastDisableTime = null;
            m_fuseTimes = 0;
        }

        public int FuseTimes => m_fuseTimes;

        public RpcEndpointDescriptor Descriptor { get; }

        public override string ToString()
        {
            return string.Concat(RpcEndpointHelper.GetIp(Host), ":", Port.ToString(), ":", ServiceProtocol.ToString());
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var endpoint = obj as IPEndPoint;
            if (endpoint != null)
                return endpoint.Address.MapToIPv4() == IPEndPoint.Address && endpoint.Port == IPEndPoint.Port;

            var model = obj as RpcEndpoint;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.Descriptor == Descriptor;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(RpcEndpoint model1, RpcEndpoint model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(RpcEndpoint model1, RpcEndpoint model2)
        {
            return !Equals(model1, model2);
        }
    }
}