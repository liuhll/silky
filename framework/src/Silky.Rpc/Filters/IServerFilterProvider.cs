using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public interface IServerFilterProvider
{
    IFilterMetadata[] GetServerFilters(ServiceEntry serviceEntry, Type instanceType);
}