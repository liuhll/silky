using System.ComponentModel.DataAnnotations;

namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class DashboardLoginInput
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不允许为空")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不允许为空")]
        [MinLength(6,ErrorMessage = "密码不允许少于6位")]
        public string Password { get; set; }
    }
}