using System.ComponentModel.DataAnnotations;
using Silky.Rpc.Auditing;

namespace ITestApplication.Account.Dtos
{
    public class LoginInput
    {
        [Required(ErrorMessage = "用户名不允许为空")] public string UserName { get; set; }

        [DisableAuditing]
        [Required(ErrorMessage = "密码不允许为空")]
        public string Password { get; set; }
    }
}