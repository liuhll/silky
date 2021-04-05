using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Account.Domain.Accounts;
using Lms.AutoMapper;
using Lms.Core.Exceptions;
using Lms.Transaction.Tcc;

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

        [TccTransaction(ConfirmMethod = "DeductBalanceConfirm", CancelMethod = "DeductBalanceCancel")]
        public async Task<long?> DeductBalance(DeductBalanceInput input)
        {
            var account = await _accountDomainService.GetAccountById(input.AccountId);
            if (input.OrderBalance > account.Balance)
            {
                throw new BusinessException("账号余额不足");
            }
            return await _accountDomainService.DeductBalance(input, TccMethodType.Try);
        }

        public Task DeductBalanceConfirm(DeductBalanceInput input)
        {
            return _accountDomainService.DeductBalance(input, TccMethodType.Confirm);
        }

        public Task DeductBalanceCancel(DeductBalanceInput input)
        {
            return _accountDomainService.DeductBalance(input, TccMethodType.Cancel);
        }
    }
}