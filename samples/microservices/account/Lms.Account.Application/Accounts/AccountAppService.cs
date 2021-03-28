using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Account.Domain.Accounts;
using Lms.AutoMapper;

namespace Lms.Account.Application.Accounts
{
    public class AccountAppService : IAccountAppService
    {
        private readonly IAccountDomainService _accountDomainService;

        public AccountAppService(IAccountDomainService accountDomainService)
        {
            _accountDomainService = accountDomainService;
        }

        public async Task<GetAccountOutput> Create(CreateAccountInput input)
        {
            var account = input.MapTo<Domain.Accounts.Account>();
            account = await _accountDomainService.Create(account);
            return account.MapTo<GetAccountOutput>();
        }
    }
}