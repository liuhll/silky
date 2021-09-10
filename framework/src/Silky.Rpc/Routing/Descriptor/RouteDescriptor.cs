using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Core.Utils;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public class RouteDescriptor
    {
        public RouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
            HostName = EngineContext.Current.HostName;
            Services = new List<ServiceDescriptor>();
            Addresses = new List<AddressDescriptor>();
        }

        public string HostName { get; set; }

        public IEnumerable<ServiceDescriptor> Services { get; set; }

        public IEnumerable<AddressDescriptor> Addresses { get; set; }

        public long TimeStamp { get; set; }

        #region Equality Members

        public override bool Equals(object obj)
        {
            var model = obj as RouteDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.HostName != HostName)
                return false;

            return model.Services.SequenceEqual(Services)
                   && model.Addresses.SequenceEqual(Addresses);
        }

        public static bool operator ==(RouteDescriptor model1, RouteDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(RouteDescriptor model1, RouteDescriptor model2)
        {
            return !Equals(model1, model2);
        }

        public override string ToString()
        {
            return HostName;
        }

        public override int GetHashCode()
        {
            return (HostName + string.Join(",", Services.Select(p => p.ToString())) +
                    string.Join(",", Addresses.Select(p => p.ToString()))).GetHashCode();
        }

        #endregion
    }
}