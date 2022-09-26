using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.Http.Core;

internal sealed partial class HttpContextServerCallContext : IServerCallContextFeature
{
    internal ILogger Logger { get; }
    internal HttpContext HttpContext { get; }
    internal ServiceEntryDescriptor ServiceEntryDescriptor { get; }

    internal ServerCallDeadlineManager? DeadlineManager;

    internal ISerializer Serializer { get; }

    private StatusCode _statusCode;
    private string? _peer;
    private Activity? _activity;
    private HttpContextSerializationContext? _serializationContext;

    internal HttpContextServerCallContext(HttpContext httpContext, ServiceEntryDescriptor serviceEntryDescriptor,
        ISerializer serializer,
        ILogger logger)
    {
        HttpContext = httpContext;
        ServiceEntryDescriptor = serviceEntryDescriptor;
        Serializer = serializer;
        Logger = logger;
    }

    public HttpContextServerCallContext ServerCallContext => this;

    protected string PeerCore
    {
        get
        {
            // Follows the standard at https://github.com/grpc/grpc/blob/master/doc/naming.md
            if (_peer == null)
            {
                _peer = BuildPeer();
            }

            return _peer;
        }
    }

    internal HttpContextSerializationContext SerializationContext
    {
        get => _serializationContext ??= new HttpContextSerializationContext(this);
    }


    public void Initialize(ISystemClock? clock = null)
    {
        _activity = GetHostActivity();
        if (_activity != null)
        {
            _activity.AddTag(HttpServerConstants.ActivityMethodTag, ServiceEntryDescriptor.Id);
        }

        SilkyRpcEventSource.Log.CallStart(ServiceEntryDescriptor.Id);
        var path = HttpContext.Request.Path;
        var method = HttpContext.Request.Method.ToEnum<HttpMethod>();
        Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
            MiniProfileConstant.Route.State.FindServiceEntry,
            $"Find the ServiceEntryDescriptor {ServiceEntryDescriptor.Id} through {path}-{method}");
        HttpContext.SetUserClaims();
        HttpContext.SetHttpHandleAddressInfo();
        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.Path, path.ToString());
        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.HttpMethod, method);
        EngineContext.Current.Resolve<ICurrentRpcToken>().SetRpcToken();

        var timeout = GetTimeout();
        if (timeout != TimeSpan.Zero)
        {
            DeadlineManager = new ServerCallDeadlineManager(this, clock ?? SystemClock.Instance, timeout);
            Logger.LogDebug($"Request deadline timeout of {0} started.", timeout);
        }
    }

    internal void WriteResponseHeaderCore()
    {
        if (HttpContext.Response.HasStarted)
        {
            throw new InvalidOperationException("Response headers can only be sent once per call.");
        }

        var gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>();
        HttpContext.Response.ContentType = HttpContext.GetResponseContentType(gatewayOptions);
        HttpContext.Response.SetHeaders();
        _statusCode = StatusCode.Success;
        HttpContext.Response.StatusCode = (int)_statusCode.GetHttpStatusCode();
        HttpContext.Response.SetResultStatusCode(_statusCode);
        HttpContext.Response.SetResultStatus((int)_statusCode);
    }

    internal void WriteResponseHeaderCore(Exception exception)
    {
        if (HttpContext.Response.HasStarted)
        {
            throw new InvalidOperationException("Response headers can only be sent once per call.");
        }

        _statusCode = exception.GetExceptionStatusCode();
        var gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>();
        HttpContext.Response.ContentType = exception is ValidationException
            ? HttpContext.GetResponseContentType(gatewayOptions)
            : "text/plain;charset=utf-8";

        HttpContext.Response.HttpContext.Features.Set(new ExceptionHandlerFeature()
        {
            Error = exception,
            Path = HttpContext.Request.Path
        });

        HttpContext.Response.SetExceptionResponseStatus(exception);
        HttpContext.Response.SetResultStatusCode(exception.GetExceptionStatusCode());
        HttpContext.Response.SetResultStatus(exception.GetExceptionStatus());
    }

    public Task EndCallAsync()
    {
        if (DeadlineManager == null)
        {
            EndCallCore();
            return Task.CompletedTask;
        }
        else if (DeadlineManager.TrySetCallComplete())
        {
            // Fast path when deadline hasn't been raised.
            EndCallCore();
            Logger.LogDebug("Request deadline stopped.");
            return DeadlineManager.DisposeAsync().AsTask();
        }

        // Deadline is exceeded
        return EndCallAsyncCore();
    }

    public async Task DeadlineExceededAsync()
    {
        SilkyRpcEventSource.Log.CallDeadlineExceeded();
        _statusCode = StatusCode.DeadlineExceeded;
        var completionFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
        if (completionFeature != null)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            await completionFeature.CompleteAsync();
        }

        var resetFeature = HttpContext.Features.Get<IHttpResetFeature>();
        if (resetFeature != null)
        {
            resetFeature.Reset(0x8);
        }
        else
        {
            HttpContext.Abort();
        }
    }

    public async Task ProcessHandlerErrorAsync(Exception ex)
    {
        if (DeadlineManager == null)
        {
            ProcessHandlerError(ex);
        }
        else
        {
            await ProcessHandlerErrorAsyncCore(ex);
        }
    }

    private async Task ProcessHandlerErrorAsyncCore(Exception ex)
    {
        Debug.Assert(DeadlineManager != null, "Deadline manager should have been created.");

        if (!DeadlineManager.TrySetCallComplete())
        {
            await DeadlineManager.WaitDeadlineCompleteAsync();
        }

        try
        {
            ProcessHandlerError(ex);
        }
        finally
        {
            await DeadlineManager.DisposeAsync();
            Logger.LogDebug("Request deadline stopped.");
        }
    }

    private void ProcessHandlerError(Exception ex)
    {
        WriteResponseHeaderCore(ex);
        DeadlineManager?.SetCallEnded();
        LogCallEnd();
    }

    private void EndCallCore()
    {
        LogCallEnd();
    }

    private void LogCallEnd()
    {
        if (_activity != null)
        {
            _activity.AddTag(HttpServerConstants.ActivityStatusCodeTag, _statusCode);
        }

        if (_statusCode != StatusCode.Success)
        {
            SilkyRpcEventSource.Log.CallFailed(_statusCode);
        }

        SilkyRpcEventSource.Log.CallStop();
    }

    private async Task EndCallAsyncCore()
    {
        Debug.Assert(DeadlineManager != null, "Deadline manager should have been created.");
        try
        {
            // Deadline has started
            await DeadlineManager.WaitDeadlineCompleteAsync();

            EndCallCore();
            DeadlineManager.SetCallEnded();
            Logger.LogDebug("Request deadline stopped.");
        }
        finally
        {
            await DeadlineManager.DisposeAsync();
        }
    }

    private TimeSpan GetTimeout()
    {
        var gatewayOptions = EngineContext.Current.GetOptions<GatewayOptions>();
        return gatewayOptions.TimeoutMillSeconds > 0
            ? TimeSpan.FromMilliseconds(gatewayOptions.TimeoutMillSeconds)
            : TimeSpan.Zero;
    }

    private string BuildPeer()
    {
        var connection = HttpContext.Connection;
        if (connection.RemoteIpAddress != null)
        {
            switch (connection.RemoteIpAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return $"ipv4:{connection.RemoteIpAddress}:{connection.RemotePort}";
                case AddressFamily.InterNetworkV6:
                    return $"ipv6:[{connection.RemoteIpAddress}]:{connection.RemotePort}";
                default:
                    // TODO(JamesNK) - Test what should be output when used with UDS and named pipes
                    return $"unknown:{connection.RemoteIpAddress}:{connection.RemotePort}";
            }
        }
        else
        {
            return "unknown"; // Match Grpc.Core
        }
    }

    private Activity? GetHostActivity()
    {
#if NET6_0_OR_GREATER
        // Feature always returns the host activity
        var feature = HttpContext.Features.Get<IHttpActivityFeature>();
        if (feature != null)
        {
            return feature.Activity;
        }
#endif

        // If feature isn't available, or not supported, then fallback to Activity.Current.
        var activity = Activity.Current;
        while (activity != null)
        {
            if (string.Equals(activity.OperationName, HttpServerConstants.HostActivityName,
                    StringComparison.Ordinal))
            {
                return activity;
            }

            activity = activity.Parent;
        }

        return null;
    }
}