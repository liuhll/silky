using System.ComponentModel.DataAnnotations;

namespace ITestApplication.Account.Dtos
{
    public class LoginInput
    {
        [Required(ErrorMessage = "用户名不允许为空")] public string UserName { get; set; }

        [Required(ErrorMessage = "密码不允许为空")] public string Password { get; set; }
    }
}