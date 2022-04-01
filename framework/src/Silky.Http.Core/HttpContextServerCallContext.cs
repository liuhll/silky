using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.Http.Core;

internal sealed partial class HttpContextServerCallContext : IServerCallContextFeature
{
    internal ILogger Logger { get; }
    internal HttpContext HttpContext { get; }
    internal ServiceEntry ServiceEntry { get; }

    internal ServerCallDeadlineManager? DeadlineManager;

    private StatusCode _statusCode;
    private string? _peer;
    private Activity? _activity;
    private HttpContextSerializationContext? _serializationContext;

    internal HttpContextServerCallContext(HttpContext httpContext, ServiceEntry serviceEntry, ILogger logger)
    {
        HttpContext = httpContext;
        ServiceEntry = serviceEntry;
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

    public WriteOptions? WriteOptions { get; set; }
    
    internal HttpContextSerializationContext SerializationContext
    {
        get => _serializationContext ??= new HttpContextSerializationContext(this);
    }
    
    public void Initialize(ISystemClock? clock = null)
    {
        _activity = GetHostActivity();
        if (_activity != null)
        {
            _activity.AddTag(HttpServerConstants.ActivityMethodTag, ServiceEntry.Id);
        }

        SilkyRpcEventSource.Log.CallStart(ServiceEntry.Id);
        var path = HttpContext.Request.Path;
        var method = HttpContext.Request.Method.ToEnum<HttpMethod>();
        Logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
            MiniProfileConstant.Route.State.FindServiceEntry,
            $"Find the ServiceEntry {ServiceEntry.Id} through {path}-{method}");
        HttpContext.SetUserClaims();
        HttpContext.SetHttpHandleAddressInfo();
        EngineContext.Current.Resolve<ICurrentRpcToken>().SetRpcToken();

        var timeout = GetTimeout();
        if (timeout != TimeSpan.Zero)
        {
            DeadlineManager = new ServerCallDeadlineManager(this, clock ?? SystemClock.Instance, timeout);
            Logger.LogDebug($"Request deadline timeout of {0} started.", timeout);
        }
    }

    internal Task WriteResponseAsyncCore(string responseData)
    {
        if (HttpContext.Response.HasStarted)
        {
            throw new InvalidOperationException("Response headers can only be sent once per call.");
        }

        HttpContext.Response.SetHeaders();
        return HttpContext.Response.BodyWriter.FlushAsync().GetAsTask();
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
        throw new NotImplementedException();
    }

    public Task ProcessHandlerErrorAsync(Exception ex)
    {
        if (DeadlineManager == null)
        {
            ProcessHandlerError(ex);
            return Task.CompletedTask;
        }

        // Could have a fast path for no deadline being raised when an error happens,
        // but it isn't worth the complexity.
        return ProcessHandlerErrorAsyncCore(ex);
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
        if (ex is SilkyException silkyException)
        {
            _statusCode = silkyException.StatusCode;
            Logger.LogWarning($"{0} => Error status code '{1}' with detail '{2}' raised.", ServiceEntry.Id, _statusCode,
                silkyException.Message);
        }
        else
        {
            Logger.LogError("Error when executing service method '{0}'.", ServiceEntry.Id);
            _statusCode = ex.GetExceptionStatusCode();
        }

        if (DeadlineManager == null || !DeadlineManager.IsDeadlineExceededStarted)
        {
            HttpContext.Response.ConsolidateTrailers(this, ex);
        }

        DeadlineManager?.SetCallEnded();

        LogCallEnd();
    }

    private void EndCallCore()
    {
        // Don't update trailers if request has exceeded deadline
        if (DeadlineManager == null || !DeadlineManager.IsDeadlineExceededStarted)
        {
            HttpContext.Response.ConsolidateTrailers(this);
        }

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
        return ServiceEntry.GovernanceOptions.TimeoutMillSeconds > 0
            ? TimeSpan.FromTicks(ServiceEntry.GovernanceOptions.TimeoutMillSeconds)
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