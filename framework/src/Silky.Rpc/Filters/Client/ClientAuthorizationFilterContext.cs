using System.Collections.Generic;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Filters;

public class ClientAuthorizationFilterContext : ClientFilterContext
{
    public ClientAuthorizationFilterContext(ClientInvokeContext context, IList<IFilterMetadata> filters) : base(context, filters)
    {
    }
    
    public virtual RemoteResultMessage? Result { get; set; }
}