using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Silky.Core.Rpc;
using Silky.Http.SkyApm.Diagnostics;
using Silky.Rpc.SkyApm.Diagnostics.Collections;
using Silky.Rpc.SkyApm.Diagnostics.Http.Extensions;
using Silky.Rpc.Transport;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.Rpc.SkyApm.Diagnostics
{
    public class HttpTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "Microsoft.AspNetCore";

        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly HostingDiagnosticConfig _config;
        private readonly TracingConfig _tracingConfig;

        public HttpTracingDiagnosticProcessor(IEntrySegmentContextAccessor entrySegmentContextAccessor,
            ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _config = configAccessor.Get<HostingDiagnosticConfig>();
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void BeginRequest([Property] HttpContext HttpContext)
        {
            var path = HttpContext.Request.Path.ToString();
            if (!PathUtils.IsWebApiPath(path))
            {
                return;
            }

            //var host = NetUtil.GetRpcAddressModel().IPEndPoint.ToString();
            var context = _tracingContext.CreateEntrySegmentContext(
                $"{HttpContext.Request.Path}-{HttpContext.Request.Method}",
                new SilkyCarrierHeaderCollection(RpcContext.Context));

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Components.SilkyHttp;
            context.Span.Peer = new StringOrIntValue(HttpContext.Connection.RemoteIpAddress.ToString());
            ;
            context.Span.AddTag(Tags.URL, HttpContext.Request.GetDisplayUrl());
            context.Span.AddTag(Tags.PATH, HttpContext.Request.Path);

            context.Span.AddTag(Tags.HTTP_METHOD, HttpContext.Request.Method);
            context.Span.AddLog(
                LogEvent.Event("Http Request Begin"),
                LogEvent.Message($"Request Starting {HttpContext.Request.Path} {HttpContext.Request.Method}"));
            if (_config.CollectCookies?.Count > 0)
            {
                var cookies = CollectCookies(HttpContext, _config.CollectCookies);
                if (!string.IsNullOrEmpty(cookies))
                    context.Span.AddTag(global::SkyApm.Common.Tags.HTTP_COOKIES, cookies);
            }

            if (_config.CollectHeaders?.Count > 0)
            {
                var headers = CollectHeaders(HttpContext, _config.CollectHeaders);
                if (!string.IsNullOrEmpty(headers))
                    context.Span.AddTag(global::SkyApm.Common.Tags.HTTP_HEADERS, headers);
            }

            if (_config.CollectBodyContentTypes?.Count > 0)
            {
                var body = CollectBody(HttpContext, _config.CollectBodyLengthThreshold);
                if (!string.IsNullOrEmpty(body))
                    context.Span.AddTag(global::SkyApm.Common.Tags.HTTP_REQUEST_BODY, body);
            }
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void EndRequest([Property] HttpContext HttpContext)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null)
            {
                return;
            }

            var statusCode = HttpContext.Response.StatusCode;
            if (statusCode >= 400)
            {
                context.Span.ErrorOccurred();
            }

            context.Span.AddLog(
                LogEvent.Event("Http Request End"),
                LogEvent.Message($"Http Request End"));
            _tracingContext.Release(context);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _entrySegmentContextAccessor.Context?.Span?.ErrorOccurred(exception, _tracingConfig);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            _entrySegmentContextAccessor.Context?.Span?.ErrorOccurred(exception, _tracingConfig);
        }

        private string CollectCookies(HttpContext httpContext, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!httpContext.Request.Cookies.TryGetValue(key, out string value))
                    continue;

                if (sb.Length > 0)
                    sb.Append("; ");

                sb.Append(key);
                sb.Append('=');
                sb.Append(value);
            }

            return sb.ToString();
        }

        private string CollectHeaders(HttpContext httpContext, IEnumerable<string> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (!httpContext.Request.Headers.TryGetValue(key, out StringValues value))
                    continue;

                if (sb.Length > 0)
                    sb.Append('\n');

                sb.Append(key);
                sb.Append(": ");
                sb.Append(value);
            }

            return sb.ToString();
        }

        private string CollectBody(HttpContext httpContext, int lengthThreshold)
        {
            var request = httpContext.Request;

            if (string.IsNullOrEmpty(httpContext.Request.ContentType)
                || httpContext.Request.ContentLength == null
                || request.ContentLength > lengthThreshold)
            {
                return null;
            }

            var contentType = new ContentType(request.ContentType);
            if (!_config.CollectBodyContentTypes.Any(supportedType => contentType.MediaType == supportedType))
                return null;

#if NETSTANDARD2_0
            httpContext.Request.EnableRewind();
#else
            httpContext.Request.EnableBuffering();
#endif
            request.Body.Position = 0;
            try
            {
                var encoding = contentType.CharSet.ToEncoding(Encoding.UTF8);
                using (var reader = new StreamReader(request.Body, encoding, true, 1024, true))
                {
                    var body = reader.ReadToEndAsync().Result;
                    return body;
                }
            }
            finally
            {
                request.Body.Position = 0;
            }
        }
    }
}