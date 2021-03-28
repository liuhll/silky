using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Account.Domain.Accounts
{
    public interface IAccountDomainService : ITransientDependency
    {
        Task<Account> Create(Account account);
    }
}