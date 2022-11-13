using Silky.Core;

namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    [CacheName("GetAccountOutput")]
    public class GetAccountOutput
    {
        /// <summary>
        /// 账号Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 账号名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// 账号余额
        /// </summary>
        public decimal Balance { get; set; }
    }
}