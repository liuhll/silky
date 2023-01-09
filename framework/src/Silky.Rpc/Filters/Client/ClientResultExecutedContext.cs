using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Filters;

public class ClientResultExecutedContext : ClientFilterContext
{
    private Exception? _exception;
    private ExceptionDispatchInfo? _exceptionDispatchInfo;
    
    public ClientResultExecutedContext(ClientInvokeContext context,
        IList<IFilterMetadata> filters,
        RemoteResultMessage result) : base(context, filters)
    {
    }
    
    public virtual RemoteResultMessage Result { get; set; }

    public virtual bool Canceled { get; set; }
    
    public virtual Exception? Exception
    {
        get
        {
            if (_exception == null && _exceptionDispatchInfo != null)
            {
                return _exceptionDispatchInfo.SourceException;
            }
            else
            {
                return _exception;
            }
        }

        set
        {
            _exceptionDispatchInfo = null;
            _exception = value;
        }
    }
    
    public virtual ExceptionDispatchInfo? ExceptionDispatchInfo
    {
        get
        {
            return _exceptionDispatchInfo;
        }

        set
        {
            _exception = null;
            _exceptionDispatchInfo = value;
        }
    }
    
    public virtual bool ExceptionHandled { get; set; }
}