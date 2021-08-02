using System.Threading.Tasks;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Core.DependencyInjection;
using Silky.Transaction.Tcc;

namespace Silky.Account.Domain.Accounts
{
    public interface IAccountDomainService : ITransientDependency
    {
        Task<Account> Create(CreateAccountInput account);
        Task<Account> GetAccountByName(string name);
        Task<Account> GetAccountById(long id);
        Task<Account> Update(UpdateAccountInput input);
        Task Delete(long id);
        Task<long?> DeductBalance(DeductBalanceInput input, TccMethodType tccMethodType);
        Task<string> Login(LoginInput input);
        Task<GetAccountOutput> GetLoginUserInfo();
    }
}