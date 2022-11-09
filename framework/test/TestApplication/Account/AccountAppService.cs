using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ITestApplication.Account;
using ITestApplication.Account.Dtos;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Jwt;

namespace TestApplication.Account
{
    public class AccountAppService : IAccountAppService
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountAppService(IJwtTokenGenerator jwtTokenGenerator)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
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
            
            RpcContext.Context.SetResponseHeader("test","test set header");
            return _jwtTokenGenerator.Generate(payload);
        }
    }
}