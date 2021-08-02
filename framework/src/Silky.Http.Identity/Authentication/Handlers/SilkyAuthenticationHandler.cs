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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Serialization;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;

namespace Silky.Http.Identity.Authentication.Handlers
{
    public class SilkyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly GatewayOptions _gatewayOptions;
        private readonly IJwtDecoder _jwtDecoder;
        private readonly ISerializer _serializer;

        public SilkyAuthenticationHandler([NotNull] [ItemNotNull] IOptionsMonitor<AuthenticationSchemeOptions> options,
            IOptions<GatewayOptions> gatewayOptions,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock,
            IJwtDecoder jwtDecoder, ISerializer serializer) :
            base(options,
                logger,
                encoder,
                clock)
        {
            _jwtDecoder = jwtDecoder;
            _serializer = serializer;
            _gatewayOptions = gatewayOptions.Value;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Task WriteErrorResponse(string message)
            {
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
                    return Context.Response.WriteAsync(responseResultData);
                }
                else
                {
                    Context.Response.ContentType = "text/plain";
                    Context.Response.ContentLength = message.GetBytes().Length;
                    Context.Response.StatusCode = ResponseStatusCode.Unauthorized;
                    return Context.Response.WriteAsync(message);
                }
            }

            var serviceEntry = Context.GetServiceEntry();
            if (serviceEntry != null)
            {
                if (serviceEntry.GovernanceOptions.IsAllowAnonymous)
                {
                    return AuthenticateResult.NoResult();
                }

                var token = Context.Request.Headers["Authorization"];
                if (token.IsNullOrEmpty())
                {
                    await WriteErrorResponse("You have not logged in to the system");
                    return AuthenticateResult.Fail(new AuthenticationException("You have not logged in to the system"));
                }

                try
                {
                    if (_gatewayOptions.JwtSecret.IsNullOrEmpty())
                    {
                        await WriteErrorResponse("You have not set JwtSecret on the Gateway configuration node, and the validity of the token cannot be verified");
                        return AuthenticateResult.Fail(new AuthenticationException(
                            "You have not set JwtSecret on the Gateway configuration node, and the validity of the token cannot be verified"));
                    }

                    var payload = _jwtDecoder.DecodeToObject(token, _gatewayOptions.JwtSecret, true);
                    var ticket = CreateTicket(payload);
                    return AuthenticateResult.Success(ticket);
                }
                catch (TokenExpiredException)
                {
                    await WriteErrorResponse("Token has expired");
                    return AuthenticateResult.Fail("Token has expired");
                }
                catch (SignatureVerificationException)
                {
                    await WriteErrorResponse("Token has invalid signature");
                    return AuthenticateResult.Fail("Token has invalid signature");
                }
                catch (Exception ex)
                {
                    await WriteErrorResponse("The token format is illegal, the reason: {ex.Message}");
                    return AuthenticateResult.Fail($"The token format is illegal, the reason: {ex.Message}");
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