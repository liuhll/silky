using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
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
    private GatewayOptions _gatewayOptions;

    internal HttpContextServerCallContext(HttpContext httpContext, ServiceEntryDescriptor serviceEntryDescriptor,
        ISerializer serializer,
        GatewayOptions gatewayOptions,
        ILogger logger)
    {
        HttpContext = httpContext;
        ServiceEntryDescriptor = serviceEntryDescriptor;
        Serializer = serializer;
        Logger = logger;
        _gatewayOptions = gatewayOptions;
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
        Logger.LogInformation(
            $"Find the ServiceEntryDescriptor {ServiceEntryDescriptor.Id} through {method} - {path}");
        HttpContext.SetUserClaims();
        HttpContext.SetHttpHandleAddressInfo();
        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.Path, path.ToString());
        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.HttpMethod, method);
        EngineContext.Current.Resolve<ICurrentRpcToken>().SetRpcToken();

        var timeout = GetTimeout(ServiceEntryDescriptor);
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

        HttpContext.Response.ContentType = HttpContext.GetResponseContentType(_gatewayOptions);
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
        HttpContext.Response.ContentType = exception is ValidationException
            ? HttpContext.GetResponseContentType(_gatewayOptions)
            : "text/plain;charset=utf-8";

        HttpContext.Response.HttpContext.Features.Set(new ExceptionHandlerFeature()
        {
            Error = exception,
            Path = HttpContext.Request.Path
        });

        HttpContext.Response.SetExceptionResponseStatus(exception);
        HttpContext.Response.SetResultStatusCode(exception.GetExceptionStatusCode());
        HttpContext.Response.SetResultStatus(exception.GetExceptionStatus());
        HttpContext.Response.WriteAsync(exception.GetExceptionMessage());
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

        CancelRequest();
    }

    internal void CancelRequest()
    {
        // HttpResetFeature should always be set on context,
        // but in case it isn't, fall back to HttpContext.Abort.
        // Abort will send error code INTERNAL_ERROR.
        var resetFeature = HttpContext.Features.Get<IHttpResetFeature>();
        if (resetFeature != null)
        {
            var errorCode = HttpProtocol.IsHttp3(HttpContext.Request.Protocol) ? 0x010c : 0x08;

            resetFeature.Reset(errorCode);
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

    protected CancellationToken CancellationTokenCore =>
        DeadlineManager?.CancellationToken ?? HttpContext.RequestAborted;

    public CancellationToken CancellationToken => CancellationTokenCore;

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

    private TimeSpan GetTimeout(ServiceEntryDescriptor serviceEntryDescriptor)
    {
        var timeoutMillSeconds = serviceEntryDescriptor.GovernanceOptions.TimeoutMillSeconds;
        return timeoutMillSeconds > 0
            ? TimeSpan.FromMilliseconds(timeoutMillSeconds)
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