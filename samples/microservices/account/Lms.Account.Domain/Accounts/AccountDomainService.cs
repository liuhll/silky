using System.Threading.Tasks;
using Arch.EntityFrameworkCore.UnitOfWork;

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
            var insertResult = await accountRepository.InsertAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return insertResult.Entity;
        }
    }
}