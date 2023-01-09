using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ServerResultExecutedContext : ServerFilterContext
{
    private Exception? _exception;
    private ExceptionDispatchInfo? _exceptionDispatchInfo;
    
    public ServerResultExecutedContext(ServiceEntryContext context, IList<IFilterMetadata> filters, object result) :
        base(context, filters)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Result = result;
    }

    public virtual bool Canceled { get; set; }

    public virtual object Result { get; }
    
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