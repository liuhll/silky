using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Account.Domain.Shared.Accounts;
using Silky.Caching;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Runtime.Session;
using Silky.EntityFrameworkCore.Repositories;
using Silky.Jwt;
using Silky.Transaction.Tcc;

namespace Silky.Account.Domain.Accounts
{
    public class AccountDomainService : IAccountDomainService
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<BalanceRecord> _balanceRecordRepository;
        private readonly IDistributedCache<GetAccountOutput, string> _accountCache;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ISession _session;

        public AccountDomainService(IRepository<Account> accountRepository,
            IDistributedCache<GetAccountOutput, string> accountCache,
            IRepository<BalanceRecord> balanceRecordRepository,
            IPasswordHelper passwordHelper,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _accountRepository = accountRepository;
            _accountCache = accountCache;
            _balanceRecordRepository = balanceRecordRepository;
            _passwordHelper = passwordHelper;
            _jwtTokenGenerator = jwtTokenGenerator;
            _session = NullSession.Instance;
        }

        public async Task<Account> Create(CreateAccountInput input)
        {
            var exsitAccountCount = await _accountRepository.CountAsync(p => p.UserName == input.UserName);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{input.UserName}名称的账号");
            }

            exsitAccountCount = await _accountRepository.CountAsync(p => p.Email == input.Email);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{input.Email}Email的账号");
            }

            var account = input.Adapt<Account>();
            account.Password = _passwordHelper.EncryptPassword(account.UserName, input.Password);
            await _accountRepository.InsertNowAsync(account);
            return account;
        }

        public async Task<Account> GetAccountByName(string name)
        {
            var accountEntry = _accountRepository.FirstOrDefault(p => p.UserName == name);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在名称为{name}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> GetAccountById(long id)
        {
            var accountEntry = _accountRepository.FirstOrDefault(p => p.Id == id);
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
                var exsitAccountCount = await _accountRepository.CountAsync(p => p.Email == input.Email);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Email为{input.Email}的账号");
                }
            }

            if (!account.UserName.Equals(input.UserName))
            {
                var exsitAccountCount = await _accountRepository.CountAsync(p => p.UserName == input.UserName);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Name为{input.UserName}的账号");
                }
            }

            await _accountCache.RemoveAsync($"Account:UserName:{account.UserName}");
            account = input.Adapt(account);
            account.Password = _passwordHelper.EncryptPassword(account.UserName, input.Password);
            await _accountRepository.UpdateAsync(account);
            return account;
        }

        public async Task Delete(long id)
        {
            var account = await GetAccountById(id);
            await _accountCache.RemoveAsync($"Account:UserName:{account.UserName}");
            await _accountCache.RemoveAsync($"CurrentUserInfo:*:{account.Id}");
            await _accountRepository.DeleteAsync(account);
        }

        public async Task<long?> DeductBalance(DeductBalanceInput input, TccMethodType tccMethodType)
        {
            var account = await GetAccountById(input.AccountId);
            await using var trans = _accountRepository.Database.BeginTransaction();
            try
            {
                BalanceRecord balanceRecord = null;
                switch (tccMethodType)
                {
                    case TccMethodType.Try:
                        account.Balance -= input.OrderBalance;
                        account.LockBalance += input.OrderBalance;
                        balanceRecord = new BalanceRecord()
                        {
                            OrderBalance = input.OrderBalance,
                            OrderId = input.OrderId,
                            PayStatus = PayStatus.NoPay
                        };
                        await _balanceRecordRepository.InsertNowAsync(balanceRecord);
                        RpcContext.Context.SetInvokeAttachment("balanceRecordId", balanceRecord.Id);
                        break;
                    case TccMethodType.Confirm:
                        account.LockBalance -= input.OrderBalance;
                        var balanceRecordId1 = RpcContext.Context.GetInvokeAttachment("orderBalanceId")?.To<long>();
                        if (balanceRecordId1.HasValue)
                        {
                            balanceRecord = await _balanceRecordRepository.FindAsync(balanceRecordId1.Value);
                            balanceRecord.PayStatus = PayStatus.Payed;
                            await _balanceRecordRepository.UpdateNowAsync(balanceRecord);
                        }

                        break;
                    case TccMethodType.Cancel:
                        account.Balance += input.OrderBalance;
                        account.LockBalance -= input.OrderBalance;
                        var balanceRecordId2 = RpcContext.Context.GetInvokeAttachment("orderBalanceId")?.To<long>();
                        if (balanceRecordId2.HasValue)
                        {
                            balanceRecord = await _balanceRecordRepository.FindAsync(balanceRecordId2.Value);
                            balanceRecord.PayStatus = PayStatus.Cancel;
                            await _balanceRecordRepository.UpdateNowAsync(balanceRecord);
                        }

                        break;
                }

                await _accountRepository.UpdateNowAsync(account);
                await trans.CommitAsync();
                await _accountCache.RemoveAsync($"Account:UserName:{account.UserName}");
                return balanceRecord?.Id;
            }
            catch (Exception e)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<string> Login(LoginInput input)
        {
            var userInfo = await _accountRepository.FirstOrDefaultAsync(p => p.UserName == input.Account
                                                                             || p.Email == input.Account);
            if (userInfo == null)
            {
                throw new AuthenticationException($"不存在账号为{input.Account}的用户");
            }

            if (!userInfo.Password.Equals(_passwordHelper.EncryptPassword(userInfo.UserName, input.Password)))
            {
                throw new AuthenticationException("密码不正确");
            }

            var payload = new Dictionary<string, object>()
            {
                { ClaimTypes.NameIdentifier, userInfo.Id },
                { ClaimTypes.Name, userInfo.UserName },
                { ClaimTypes.Email, userInfo.Email },
            };
            return _jwtTokenGenerator.Generate(payload);
        }

        public async Task<GetAccountOutput> GetLoginUserInfo()
        {
            Debug.Assert(_session.IsLogin());
            var userInfo = await _accountRepository.FindOrDefaultAsync(_session.UserId.To<long>());
            if (userInfo == null)
            {
                throw new AuthenticationException($"当前系统不存在用户{_session.UserId}");
            }

            return userInfo.Adapt<GetAccountOutput>();
        }
        
    }
}