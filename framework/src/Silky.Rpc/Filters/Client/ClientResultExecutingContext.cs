using System.Collections.Generic;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Filters;

public class ClientResultExecutingContext : ClientFilterContext
{
    public ClientResultExecutingContext(ClientInvokeContext context, 
        IList<IFilterMetadata> filters,
        RemoteResultMessage result) 
        : base(context, filters)
    {
        Result = result;
    }

    public virtual RemoteResultMessage Result { get; set; }

    public virtual bool Cancel { get; set; }
}