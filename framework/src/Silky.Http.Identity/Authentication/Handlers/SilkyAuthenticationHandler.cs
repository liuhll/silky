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
using Silky.Http.Core.Configuration;

namespace Silky.Http.Identity.Authentication.Handlers
{
    internal class SilkyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private GatewayOptions _gatewayOptions;
        private readonly IJwtDecoder _jwtDecoder;

        public SilkyAuthenticationHandler(
            [NotNull] [ItemNotNull] IOptionsMonitor<AuthenticationSchemeOptions> authenticationSchemeOptions,
            IOptionsMonitor<GatewayOptions> gatewayOptions,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock,
            IJwtDecoder jwtDecoder) :
            base(authenticationSchemeOptions,
                logger,
                encoder,
                clock)
        {
            _jwtDecoder = jwtDecoder;
            _gatewayOptions = gatewayOptions.CurrentValue;
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = GetAuthorizationToken(Context);
            if (token.IsNullOrEmpty())
            {
                return AuthenticateResult.Fail(new AuthenticationException("You have not logged in to the system."));
            }

            try
            {
                if (_gatewayOptions.JwtSecret.IsNullOrEmpty())
                {
                    return AuthenticateResult.Fail(new AuthenticationException(
                        "You have not set JwtSecret on the Gateway configuration node, and the validity of the token cannot be verified"));
                }

                var payload = _jwtDecoder.DecodeToObject(token, _gatewayOptions.JwtSecret, true);
                var ticket = CreateTicket(payload);
                return AuthenticateResult.Success(ticket);
            }
            catch (TokenExpiredException ex)
            {
                throw new AuthenticationException("Token has expired");
            }
            catch (SignatureVerificationException ex)
            {
                throw new AuthenticationException("Token has invalid signature");
            }
            catch (Exception ex)
            {
                throw new AuthenticationException($"The token format is illegal, the reason: {ex.Message}");
            }
        }

        private string GetAuthorizationToken(HttpContext httpContext, string headerKey = "Authorization",
            string tokenPrefix = "Bearer ")
        {
            var bearerToken = httpContext.Request.Headers[headerKey].ToString();
            if (string.IsNullOrWhiteSpace(bearerToken)) return default;

            var prefixLenght = tokenPrefix.Length;
            return bearerToken.StartsWith(tokenPrefix, true, null) && bearerToken.Length > prefixLenght
                ? bearerToken[prefixLenght..]
                : bearerToken;
        }

        private AuthenticationTicket CreateTicket(IDictionary<string, object> payload)
        {
            var claimsIdentity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            foreach (var item in payload)
            {
                if (item.Key.Equals(ClaimTypes.Role))
                {
                    var roles = item.Value.ToString().Split(",");
                    var roleClaims = roles.Select(p => new Claim(item.Key, p));
                    claimsIdentity.AddClaims(roleClaims);
                }
                else
                {
                    claimsIdentity.AddClaim(new Claim(item.Key, item.Value.ToString()));
                }
            }

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);
            return ticket;
        }
    }
}