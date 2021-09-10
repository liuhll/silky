﻿using System.Threading.Tasks;

namespace Silky.Rpc.Address
{
    public delegate Task HealthChangeEvent(IAddressModel addressModel, bool isHealth);

    public delegate Task UnhealthEvent(IAddressModel addressModel);

    public delegate Task RemoveAddressEvent(IAddressModel addressModel);

    public delegate Task AddMonitorEvent(IAddressModel addressModel);

    // public delegate Task RemoveServiceRouteAddressEvent(string serviceId, IAddressModel addressModel);
}