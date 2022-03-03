using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Http.Dashboard.Configuration;

namespace Silky.Http.Dashboard.Middlewares
{
    public class UiMiddleware
    {
        private const string EmbeddedFileNamespace = "Silky.Http.Dashboard.node_modules.silky_dashboard_ui_dist";
        private const string PathMatch = "/dashboard";
        private readonly DashboardOptions _options;
        private readonly Regex _redirectUrlCheckRegex;
        private readonly Regex _homeUrlCheckRegex;
        private readonly StaticFileMiddleware _staticFileMiddleware;

        public UiMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, ILoggerFactory loggerFactory,
            IOptions<DashboardOptions> options)

        {
            _options = options.Value;
            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, _options);
            _redirectUrlCheckRegex = new Regex($"^/?{Regex.Escape(PathMatch)}/?$", RegexOptions.IgnoreCase);
            _homeUrlCheckRegex =
                new Regex($"^/?{Regex.Escape(PathMatch)}/?index.html$", RegexOptions.IgnoreCase);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            if (httpMethod == "GET")
            {
                if (_redirectUrlCheckRegex.IsMatch(path))
                {
                    var redirectUrl = string.IsNullOrEmpty(path) || path.EndsWith("/")
                        ? "index.html"
                        : $"{path.Split('/').Last()}/index.html";

                    httpContext.Response.StatusCode = 301;
                    httpContext.Response.Headers["Location"] = redirectUrl;
                    return;
                }

                if (_homeUrlCheckRegex.IsMatch(path))
                {
                    httpContext.Response.StatusCode = 200;
                    httpContext.Response.ContentType = "text/html;charset=utf-8";

                    await using var stream = GetType().Assembly
                        .GetManifestResourceStream(EmbeddedFileNamespace + ".index.html");
                    if (stream == null) throw new InvalidOperationException();

                    using var sr = new StreamReader(stream);
                    var htmlBuilder = new StringBuilder(await sr.ReadToEndAsync());
                    htmlBuilder.Replace("%(servicePrefix)", _options.PathBase + "/api/silky");
                    htmlBuilder.Replace("%(useAuth)", _options.UseAuth.ToString());
                    htmlBuilder.Replace("%(wrapperResponse)", _options.WrapperResponse.ToString());
                    htmlBuilder.Replace("%(useTenant)", _options.UseTenant.ToString());
                    htmlBuilder.Replace("%(tenantName)", _options.TenantNameFiledName);
                    htmlBuilder.Replace("%(userName)", _options.UserNameFiledName);
                    htmlBuilder.Replace("%(password)", _options.PasswordFiledName);
                    if (_options.UseAuth)
                    {
                        var loginWebApi = _options.DashboardLoginApi.StartsWith("/")
                            ? _options.DashboardLoginApi
                            : "/" + _options.DashboardLoginApi;
                        htmlBuilder.Replace("%(authApi)", _options.PathBase + loginWebApi);
                    }
                    else
                    {
                        htmlBuilder.Replace("%(authApi)", "");
                    }

                    htmlBuilder.Replace("%(pollingInterval)", _options.StatsPollingInterval.ToString());
                    await httpContext.Response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);

                    return;
                }
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory, DashboardOptions options)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = PathMatch,
                FileProvider =
                    new EmbeddedFileProvider(typeof(UiMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }
    }
}