using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transport.CachingIntercept;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// 通过账号名称获取账号
        /// </summary>
        /// <param name="name">账号名称</param>
        /// <returns></returns>
        [GetCachingIntercept("Account:Name:{0}")]
        [HttpGet("{name:string}")]
        Task<GetAccountOutput> GetAccountByName([CacheKey(0)] string name);

        /// <summary>
        /// 通过Id获取账号信息
        /// </summary>
        /// <param name="id">账号Id</param>
        /// <returns></returns>
        [GetCachingIntercept("Account:Id:{0}")]
        [HttpGet("{id:long}")]
        Task<GetAccountOutput> GetAccountById([CacheKey(0)] long id);

        /// <summary>
        /// 更新账号信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [UpdateCachingIntercept( "Account:Id:{0}")]
        Task<GetAccountOutput> Update(UpdateAccountInput input);
    }
}