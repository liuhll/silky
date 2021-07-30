using System.Threading.Tasks;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Tcc;

namespace Silky.Account.Domain.Accounts
{
    public interface IAccountDomainService : ITransientDependency
    {
        Task<Account> Create(Account account);
        Task<Account> GetAccountByName(string name);
        Task<Account> GetAccountById(long id);
        Task<Account> Update(UpdateAccountInput input);
        Task Delete(long id);
        Task<long?> DeductBalance(DeductBalanceInput input, MethodType tccMethodType);
    }
}