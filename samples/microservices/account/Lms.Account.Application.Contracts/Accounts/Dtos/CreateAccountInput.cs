using System.ComponentModel.DataAnnotations;

namespace Lms.Account.Application.Contracts.Accounts.Dtos
{
    public class CreateAccountInput
    {
        /// <summary>
        /// 账号/姓名
        /// </summary>
        [Required(ErrorMessage = "名称不允许为空")]
        public string Name { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        [Required(ErrorMessage = "Email不允许为空")]
        [EmailAddress(ErrorMessage = "Email格式不正确")]
        public string Email { get; set; }
        
        /// <summary>
        /// 账号余额
        /// </summary>
        public decimal Balance { get; set; }
    }
}