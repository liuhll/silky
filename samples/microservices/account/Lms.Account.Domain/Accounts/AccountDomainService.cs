using System.Threading.Tasks;
using Arch.EntityFrameworkCore.UnitOfWork;
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
    }
}