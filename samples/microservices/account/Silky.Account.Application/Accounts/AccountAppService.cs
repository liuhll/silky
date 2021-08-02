using System.Threading.Tasks;
using DotNetCore.CAP;
using Mapster;
using Silky.Account.Application.Contracts.Accounts;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Account.Domain.Accounts;
using Silky.Core.Exceptions;
using Silky.Transaction.Tcc;

namespace Silky.Account.Application.Accounts
{
    public class AccountAppService : IAccountAppService, ICapSubscribe
    {
        private readonly IAccountDomainService _accountDomainService;
        private readonly ICapPublisher _capBus;

        public AccountAppService(IAccountDomainService accountDomainService, ICapPublisher capBus)
        {
            _accountDomainService = accountDomainService;
            _capBus = capBus;
        }

        public async Task<GetAccountOutput> Create(CreateAccountInput input)
        {
            var account = await _accountDomainService.Create(input);
            return account.Adapt<GetAccountOutput>();
        }

        public Task<string> Login(LoginInput input)
        {
            return _accountDomainService.Login(input);
        }

        public async Task<GetAccountOutput> GetAccountByName(string name)
        {
            var account = await _accountDomainService.GetAccountByName(name);
            return account.Adapt<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> GetAccountById(long id)
        {
            var account = await _accountDomainService.GetAccountById(id);
            return account.Adapt<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> Update(UpdateAccountInput input)
        {
            var account = await _accountDomainService.Update(input);
            return account.Adapt<GetAccountOutput>();
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