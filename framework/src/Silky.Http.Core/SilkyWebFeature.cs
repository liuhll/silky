using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core;

public class SilkyWebFeature :
    IRequestBodyPipeFeature,
    IHttpResponseBodyFeature,
    IHttpResponseTrailersFeature,
    IHttpResetFeature
{
    private readonly IHttpResponseBodyFeature _initialResponseFeature;
    private readonly IRequestBodyPipeFeature? _initialRequestFeature;
    private readonly IHttpResetFeature? _initialResetFeature;
    private readonly IHttpResponseTrailersFeature? _initialTrailersFeature;
    private bool _isComplete;

    public SilkyWebFeature(ServiceEntry serviceEntry, HttpContext httpContext)
    {
        _initialRequestFeature = httpContext.Features.Get<IRequestBodyPipeFeature>();
        _initialResponseFeature = GetRequiredFeature<IHttpResponseBodyFeature>(httpContext);
        _initialResetFeature = httpContext.Features.Get<IHttpResetFeature>();
        _initialTrailersFeature = httpContext.Features.Get<IHttpResponseTrailersFeature>();

        var innerReader = _initialRequestFeature?.Reader ?? httpContext.Request.BodyReader;
        var innerWriter = _initialResponseFeature.Writer ?? httpContext.Response.BodyWriter;

        Trailers = new HeaderDictionary();
        Reader = innerReader;
        Writer = innerWriter;
        ServiceEntry = serviceEntry;

        httpContext.Features.Set<IRequestBodyPipeFeature>(this);
        httpContext.Features.Set<IHttpResponseBodyFeature>(this);
        httpContext.Features.Set<IHttpResponseTrailersFeature>(this);
        httpContext.Features.Set<IHttpResetFeature>(this);
    }

    public PipeReader Reader { get; }

    public ServiceEntry ServiceEntry { get; }

    public void DisableBuffering() => _initialResponseFeature.DisableBuffering();

    public Task StartAsync(CancellationToken cancellationToken) =>
        _initialResponseFeature.StartAsync(cancellationToken);

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Sending a file during a  http request call is not supported.");

    public async Task CompleteAsync()
    {
        await WriteTrailersAsync();
        await _initialResponseFeature.CompleteAsync();
        _isComplete = true;
    }

    public Stream Stream => _initialResponseFeature.Stream;
    // throw new NotSupportedException("Writing to the response stream during a http request is not supported.");

    public PipeWriter Writer { get; }
    public IHeaderDictionary Trailers { get; set; }

    public void Reset(int errorCode)
    {
        _initialResetFeature?.Reset(errorCode);
    }

    internal void DetachFromContext(HttpContext httpContext)
    {
        httpContext.Features.Set<IRequestBodyPipeFeature>(_initialRequestFeature!);
        httpContext.Features.Set<IHttpResponseBodyFeature>(_initialResponseFeature!);
        httpContext.Features.Set<IHttpResponseTrailersFeature>(_initialTrailersFeature!);
        httpContext.Features.Set<IHttpResetFeature>(_initialResetFeature!);
    }

    private static T GetRequiredFeature<T>(HttpContext httpContext)
    {
        var feature = httpContext.Features.Get<T>();
        if (feature == null)
        {
            throw new InvalidOperationException($"Couldn't get {typeof(T).FullName} from the current request.");
        }

        return feature;
    }

    public Task WriteTrailersAsync()
    {
        if (!_isComplete && Trailers.Count > 0)
        {
            return WebProtocolHelpers.WriteTrailersAsync(Trailers, Writer);
        }

        return Task.CompletedTask;
    }
}