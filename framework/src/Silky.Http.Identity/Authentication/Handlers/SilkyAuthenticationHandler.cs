using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JWT;
using JWT.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Configuration;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Serialization;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Identity.Extensions;

namespace Silky.Http.Identity.Authentication.Handlers
{
    internal class SilkyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private GatewayOptions _gatewayOptions;
        private AppSettingsOptions _appSettingsOptions;
        private readonly IJwtDecoder _jwtDecoder;
        private readonly ISerializer _serializer;

        public SilkyAuthenticationHandler(
            [NotNull] [ItemNotNull] IOptionsMonitor<AuthenticationSchemeOptions> authenticationSchemeOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            IOptionsMonitor<AppSettingsOptions> appSettingOptions,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock,
            IJwtDecoder jwtDecoder, ISerializer serializer) :
            base(authenticationSchemeOptions,
                logger,
                encoder,
                clock)
        {
            _jwtDecoder = jwtDecoder;
            _serializer = serializer;
            _gatewayOptions = gatewayOptions.CurrentValue;
            _appSettingsOptions = appSettingOptions.CurrentValue;
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
            appSettingOptions.OnChange((options, s) => _appSettingsOptions = options);
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            async Task<AuthenticateResult> WriteErrorAndReturnAuthenticateResult(string message, Exception ex)
            {
                if (ex != null && _appSettingsOptions.DisplayFullErrorStack)
                {
                    message += Environment.NewLine + ex.StackTrace;
                }

                if (_gatewayOptions.WrapResult)
                {
                    Context.Response.ContentType = "application/json;charset=utf-8";
                    var responseResultDto = new ResponseResultDto()
                    {
                        Status = StatusCode.UnAuthentication,
                        ErrorMessage = message
                    };


                    var responseResultData = _serializer.Serialize(responseResultDto);
                    Context.Response.ContentLength = responseResultData.GetBytes().Length;
                    Context.Response.StatusCode = ResponseStatusCode.Success;
                    await Context.Response.WriteAsync(responseResultData);
                }
                else
                {
                    Context.Response.ContentType = "text/plain";
                    Context.Response.ContentLength = message.GetBytes().Length;
                    Context.Response.StatusCode = ResponseStatusCode.Unauthorized;
                    await Context.Response.WriteAsync(message);
                }

                return AuthenticateResult.Fail(new AuthenticationException(message));
            }

            var serviceEntry = Context.GetServiceEntry();
            if (serviceEntry != null)
            {
                var silkyAppServiceUseAuth =
                    EngineContext.Current.Configuration.GetValue<bool?>("dashboard:useAuth") ?? false;
                if (serviceEntry.IsSilkyAppService() && !silkyAppServiceUseAuth)
                {
                    return AuthenticateResult.NoResult();
                }

                if (serviceEntry.GovernanceOptions.IsAllowAnonymous)
                {
                    return AuthenticateResult.NoResult();
                }

                var token = Context.Request.Headers["Authorization"];
                if (token.IsNullOrEmpty())
                {
                    return await WriteErrorAndReturnAuthenticateResult("You have not logged in to the system", null);
                }

                try
                {
                    if (_gatewayOptions.JwtSecret.IsNullOrEmpty())
                    {
                        return await WriteErrorAndReturnAuthenticateResult(
                            "You have not set JwtSecret on the Gateway configuration node, and the validity of the token cannot be verified",
                            null);
                    }

                    var payload = _jwtDecoder.DecodeToObject(token, _gatewayOptions.JwtSecret, true);
                    var ticket = CreateTicket(payload);
                    return AuthenticateResult.Success(ticket);
                }
                catch (TokenExpiredException ex)
                {
                    await WriteErrorAndReturnAuthenticateResult("Token has expired", ex);
                    return AuthenticateResult.Fail("Token has expired");
                }
                catch (SignatureVerificationException ex)
                {
                    return await WriteErrorAndReturnAuthenticateResult("Token has invalid signature", ex);
                }
                catch (Exception ex)
                {
                    return await WriteErrorAndReturnAuthenticateResult(
                        $"The token format is illegal, the reason: {ex.Message}", ex);
                }
            }

            return AuthenticateResult.NoResult();
        }

        private AuthenticationTicket CreateTicket(IDictionary<string, object> payload)
        {
            var claimsIdentity = new ClaimsIdentity(payload.Select(p => new Claim(p.Key, p.Value.ToString())),
                JwtBearerDefaults.AuthenticationType);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);
            return ticket;
        }
    }
}