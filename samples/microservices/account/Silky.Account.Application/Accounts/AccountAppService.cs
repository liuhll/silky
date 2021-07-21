using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Silky.Account.Application.Contracts.Accounts;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Account.Domain.Accounts;
using Silky.AutoMapper;
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
           
            var account = input.MapTo<Domain.Accounts.Account>();
            account = await _accountDomainService.Create(account);
            await _capBus.PublishAsync("account.create.time", DateTime.Now);
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