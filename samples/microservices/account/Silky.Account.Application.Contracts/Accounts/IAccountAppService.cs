using System.Threading.Tasks;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;
using Silky.Transaction;

namespace Silky.Account.Application.Contracts.Accounts
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
        /// 登陆接口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AllowAnonymous]
        Task<string> Login(LoginInput input);
        
        [AllowAnonymous]
        [HttpPost("dashboard/login")]
        Task<string> DashboardLogin(DashboardLoginInput input);

        /// <summary>
        /// 获取当前登陆用户
        /// </summary>
        /// <returns></returns>
        [HttpGet("current/userinfo")]
        [GetCachingIntercept("CurrentUserInfo",OnlyCurrentUserData = true)]
        Task<GetAccountOutput> GetLoginUserInfo();

        /// <summary>
        /// 通过账号名称获取账号
        /// </summary>
        /// <param name="name">账号名称</param>
        /// <returns></returns>
        [GetCachingIntercept("Account:UserName:{0}")]
        [HttpGet("{name}")]
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

        /// <summary>
        /// 删除账号信息
        /// </summary>
        /// <param name="id">账号Id</param>
        /// <returns></returns>
        [RemoveCachingIntercept("GetAccountOutput","Account:Id:{0}")]
        [HttpDelete("{id:long}")]
        Task Delete([CacheKey(0)]long id);

        /// <summary>
        /// 订单扣款
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Governance(ProhibitExtranet = true)]
        [RemoveCachingIntercept("GetAccountOutput","Account:Id:{0}")]
        [Transaction]
        Task<long?> DeductBalance(DeductBalanceInput input);
    }
}