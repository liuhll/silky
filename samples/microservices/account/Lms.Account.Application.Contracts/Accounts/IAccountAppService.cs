using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace Lms.Account.Application.Contracts.Accounts
{
    [ServiceRoute]
    public interface IAccountAppService
    {
        Task<GetAccountOutput> Create(CreateAccountInput input);
    }
}