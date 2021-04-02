using System.Threading.Tasks;
using Arch.EntityFrameworkCore.UnitOfWork;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.AutoMapper;
using Lms.Core.Exceptions;

namespace Lms.Account.Domain.Accounts
{
    public class AccountDomainService : IAccountDomainService
    {
        // private readonly IRepository<Account> _accountRepository;
        //
        // public AccountDomainService(IRepository<Account> accountRepository)
        // {
        //     _accountRepository = accountRepository;
        // }
        private readonly IUnitOfWork _unitOfWork;

        public AccountDomainService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Account> Create(Account account)
        {
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var exsitAccountCount = accountRepository.Count(p => p.Name == account.Name);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Name}名称的账号");
            }

            exsitAccountCount = accountRepository.Count(p => p.Email == account.Email);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Email}Email的账号");
            }
            var insertResult = await accountRepository.InsertAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return insertResult.Entity;
        }

        public async Task<Account> GetAccountByName(string name)
        {
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var accountEntry = await accountRepository.GetFirstOrDefaultAsync(predicate: p=> p.Name == name);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在名称为{name}的账号");
            }
            return accountEntry;
        }

        public async Task<Account> GetAccountById(long id)
        {
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var accountEntry = await accountRepository.GetFirstOrDefaultAsync(predicate: p=> p.Id == id);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在Id为{id}的账号");
            }
            return accountEntry;
        }

        public async Task<Account> Update(UpdateAccountInput input)
        {
            var account = await GetAccountById(input.Id);
            account = input.MapTo(account);
            var accountRepository = _unitOfWork.GetRepository<Account>(); 
            accountRepository.Update(account);
            await _unitOfWork.SaveChangesAsync();
            return account;
        }
    }
}