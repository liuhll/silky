using System;
using System.Collections.Generic;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Jwt.Configuration;

namespace Silky.Jwt
{
    public class JwtTokenGenerator : ITransientDependency, IJwtTokenGenerator
    {
        private JwtOptions _jwtOptions;
        private IJwtAlgorithm _jwtAlgorithm;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions, IJwtAlgorithm jwtAlgorithm)
        {
            _jwtOptions = jwtOptions.Value;
            _jwtAlgorithm = jwtAlgorithm;
        }

        public string Generate(IDictionary<string, object> payload)
        {
            VerifyJwtOptions();
            return JwtBuilder
                .Create()
                .WithAlgorithm(_jwtAlgorithm)
                .WithSecret(_jwtOptions.Secret)
                .AddClaim(ClaimName.Issuer, _jwtOptions.Issuer)
                .AddClaim(ClaimName.Audience, _jwtOptions.Audience)
                .AddClaim(ClaimName.IssuedAt, DateTime.Now)
                .AddClaim(ClaimName.ExpirationTime,DateTimeOffset.UtcNow.AddHours(_jwtOptions.ExpiredTime).ToUnixTimeSeconds())
                .AddClaims(payload)
                .Encode();
        }

        private void VerifyJwtOptions()
        {
            if (_jwtOptions.Issuer.IsNullOrEmpty())
            {
                throw new SilkyException("The Issuer configuration for issuing Jwt tokens is not allowed to be empty",
                    StatusCode.IssueTokenError);
            }

            if (_jwtOptions.Audience.IsNullOrEmpty())
            {
                throw new SilkyException("The Audience configuration for issuing Jwt tokens is not allowed to be empty",
                    StatusCode.IssueTokenError);
            }

            if (_jwtOptions.Secret.IsNullOrEmpty())
            {
                throw new SilkyException("The Secret configuration for issuing Jwt tokens is not allowed to be empty",
                    StatusCode.IssueTokenError);
            }

            if (_jwtOptions.ExpiredTime <= 0)
            {
                throw new SilkyException("The ExpiredTime configuration for issuing Jwt token must be greater than 0",
                    StatusCode.IssueTokenError);
            }
        }
    }
}