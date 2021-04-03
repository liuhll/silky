using System.Linq;
using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.AutoMapper;
using Lms.Caching;
using Lms.Core.Exceptions;
using TanvirArjel.EFCore.GenericRepository;

namespace Lms.Account.Domain.Accounts
{
    public class AccountDomainService : IAccountDomainService
    {
        private readonly IRepository _repository;
        private readonly IDistributedCache<GetAccountOutput, string> _accountCache;

        public AccountDomainService(IRepository repository,
            IDistributedCache<GetAccountOutput, string> accountCache)
        {
            _repository = repository;
            _accountCache = accountCache;
        }

        public async Task<Account> Create(Account account)
        {
            var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Name == account.Name);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Name}名称的账号");
            }

            exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Email == account.Email);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Email}Email的账号");
            }

            await _repository.InsertAsync<Account>(account);
            return account;
        }

        public async Task<Account> GetAccountByName(string name)
        {
            var accountEntry = _repository.GetQueryable<Account>().FirstOrDefault(p => p.Name == name);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在名称为{name}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> GetAccountById(long id)
        {
            var accountEntry = _repository.GetQueryable<Account>().FirstOrDefault(p => p.Id == id);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在Id为{id}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> Update(UpdateAccountInput input)
        {
            var account = await GetAccountById(input.Id);
            if (!account.Email.Equals(input.Email))
            {
                var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Email == input.Email);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Email为{input.Email}的账号");
                }
            }

            if (!account.Name.Equals(input.Name))
            {
                var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Name == input.Name);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Name为{input.Name}的账号");
                }
            }

            account = input.MapTo(account);
            await _repository.UpdateAsync(account);
            await _accountCache.RemoveAsync($"Account:Name:{input.Name}");
            return account;
        }
    }
}