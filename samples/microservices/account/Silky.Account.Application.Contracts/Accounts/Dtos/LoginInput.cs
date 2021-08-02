using System.ComponentModel.DataAnnotations;

namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class LoginInput
    {
        /// <summary>
        /// 账号:UserName || Email
        /// </summary>
        [Required(ErrorMessage = "账号不允许为空")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不允许为空")]
        [MinLength(6,ErrorMessage = "密码不允许少于6位")]
        public string Password { get; set; }
    }
}