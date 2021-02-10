using System;
using System.Net;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Address
{
    public class AddressModel : IAddressModel
    {
        private int m_fuseTimes;
        public AddressModel(
            [NotNull] string address,
            [NotNull] int port,
            ServiceProtocol serviceProtocol = ServiceProtocol.Tcp
        )
        {
            Check.NotNull(address, nameof(address));
            Address = address;
            Port = port;
            ServiceProtocol = serviceProtocol;
            m_fuseTimes = 0;
            Descriptor = new AddressDescriptor()
                {Address = Address, Port = Port, ServiceProtocol = ServiceProtocol};
        }

        public string Address { get; }

        public int Port { get; }

        public ServiceProtocol ServiceProtocol { get; }

        public IPEndPoint IPEndPoint => new(IPAddress.Parse(AddressHelper.GetIp(Address)), Port);

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
            m_fuseTimes = 0;
        }

        public int FuseTimes => m_fuseTimes;

        public AddressDescriptor Descriptor { get; }

        public override string ToString()
        {
            return string.Concat(AddressHelper.GetIp(Address), ":", Port.ToString(), ":", ServiceProtocol.ToString());
        }

        public override bool Equals([CanBeNull]object obj)
        {
            var endpoint = obj as IPEndPoint;
            if (endpoint != null)
                return endpoint.Address.MapToIPv4() == IPEndPoint.Address && endpoint.Port == IPEndPoint.Port;
            
            var model = obj as AddressModel;
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

        public static bool operator ==(AddressModel model1, AddressModel model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(AddressModel model1, AddressModel model2)
        {
            return !Equals(model1, model2);
        }
    }
}