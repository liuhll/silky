using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITestApplication.Account;
using ITestApplication.Account.Dtos;
using Silky.Core.Exceptions;
using Silky.Jwt;
using Silky.Rpc.Security;

namespace NormHostDemo.Account
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

            var userIdRandom = new Random((int) DateTime.Now.Ticks);
            var userId = userIdRandom.Next(1, 10000);
            var payload = new Dictionary<string, object>()
            {
                {ClaimTypes.UserId, userId},
                {ClaimTypes.UserName, input.UserName}
            };
            return _jwtTokenGenerator.Generate(payload);
        }
    }
}