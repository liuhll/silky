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

        public async Task<GetAccountOutput> GetAccountByName(string name)
        {
            var account = await _accountDomainService.GetAccountByName(name);
            return account.MapTo<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> GetAccountById(long id)
        {
            var account = await _accountDomainService.GetAccountById(id);
            return account.MapTo<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> Update(UpdateAccountInput input)
        {
            var account = await _accountDomainService.Update(input);
            return account.MapTo<GetAccountOutput>();
        }

        public Task Delete(long id)
        {
            return _accountDomainService.Delete(id);
        }
    }
}