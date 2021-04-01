using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace Lms.Account.Application.Contracts.Accounts
{
    /// <summary>
    /// 账号服务
    /// </summary>
    [ServiceRoute]
    public interface IAccountAppService
    {
        /// <summary>
        /// 新增账号
        /// </summary>
        /// <param name="input">账号信息</param>
        /// <returns></returns>
        Task<GetAccountOutput> Create(CreateAccountInput input);
    }
}