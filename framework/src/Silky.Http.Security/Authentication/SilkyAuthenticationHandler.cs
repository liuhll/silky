using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JWT;
using JWT.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Security.Authentication
{
    public class SilkyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly GatewayOptions _gatewayOptions;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IJwtDecoder _jwtDecoder;

        public SilkyAuthenticationHandler([NotNull] [ItemNotNull] IOptionsMonitor<AuthenticationSchemeOptions> options,
            IOptions<GatewayOptions> gatewayOptions,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock,
            IServiceEntryLocator serviceEntryLocator,
            IJwtDecoder jwtDecoder) :
            base(options,
                logger,
                encoder,
                clock)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _jwtDecoder = jwtDecoder;
            _gatewayOptions = gatewayOptions.Value;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Context.User = null!;
            var path = Context.Request.Path;
            var method = Context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = _serviceEntryLocator.GetServiceEntryByApi(path, method);
            if (serviceEntry != null)
            {
                if (serviceEntry.GovernanceOptions.IsAllowAnonymous)
                {
                    return AuthenticateResult.NoResult();
                }

                var token = Context.Request.Headers["Authorization"];
                if (token.IsNullOrEmpty())
                {
                    return AuthenticateResult.Fail(new AuthenticationException("You have not logged in to the system"));
                }

                try
                {
                    if (_gatewayOptions.JwtSecret.IsNullOrEmpty())
                    {
                        return AuthenticateResult.Fail(new AuthenticationException(
                            "You have not set JwtSecret on the Gateway configuration node, and the validity of the token cannot be verified"));
                    }

                    var payload = _jwtDecoder.DecodeToObject(token, _gatewayOptions.JwtSecret, true);
                }
                catch (TokenExpiredException)
                {
                    return AuthenticateResult.Fail("Token has expired");
                }
                catch (SignatureVerificationException)
                {
                    return AuthenticateResult.Fail("Token has invalid signature");
                }
                catch (Exception ex)
                {
                    return AuthenticateResult.Fail($"The token format is illegal, the reason: {ex.Message}");
                }
            }

            return AuthenticateResult.NoResult();
        }
    }
}