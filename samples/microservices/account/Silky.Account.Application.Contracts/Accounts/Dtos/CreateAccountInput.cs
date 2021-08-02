using System.ComponentModel.DataAnnotations;

namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class CreateAccountInput
    {
        /// <summary>
        /// 账号/姓名
        /// </summary>
        [Required(ErrorMessage = "名称不允许为空")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不允许为空")]
        [MinLength(6,ErrorMessage = "密码不允许少于6位")]
        public string Password { get; set; }

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