using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ITestApplication.Account;
using ITestApplication.Account.Dtos;
using Mapster;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Runtime.Session;
using Silky.Jwt;
using Silky.Rpc.Configuration;

namespace TestApplication.Account
{
    public class AccountAppService : IAccountAppService
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ISession _session;

        public AccountAppService(IJwtTokenGenerator jwtTokenGenerator)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _session = NullSession.Instance;
        }

        public async Task<string> Login(LoginInput input)
        {
            if (!input.Password.Equals("123qwe"))
            {
                throw new AuthenticationException("密码不正确");
            }

            var userIdRandom = new Random((int)DateTime.Now.Ticks);
            var userId = userIdRandom.Next(1, 10000);
            var payload = new Dictionary<string, object>()
            {
                { ClaimTypes.NameIdentifier, userId },
                { ClaimTypes.Name, input.UserName },
                { ClaimTypes.Role, "PowerUser,Dashboard" }
            };

            RpcContext.Context.SetResponseHeader("test", "test set header");
            var token = _jwtTokenGenerator.Generate(payload);
            RpcContext.Context.SigninToSwagger(token);
            return token;
        }

        public async Task<int> CheckUrl()
        {
            if (!_session.IsLogin())
            {
                return 401;
            }

            return 200;
        }

        public async Task<int> SubmitUrl(SpecificationWithTenantAuth authInfo)
        {
            var loginInput = authInfo.Adapt<LoginInput>();
            var token = await Login(loginInput);
            RpcContext.Context.SigninToSwagger(token);
            return 200;
        }
    }
}