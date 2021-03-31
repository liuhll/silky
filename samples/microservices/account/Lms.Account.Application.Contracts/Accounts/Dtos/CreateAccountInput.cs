using System.ComponentModel.DataAnnotations;

namespace Lms.Account.Application.Contracts.Accounts.Dtos
{
    public class CreateAccountInput
    {
        [Required(ErrorMessage = "名称不允许为空")]
        public string Name { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Email不允许为空")]
        [EmailAddress(ErrorMessage = "Email格式不正确")]
        public string Email { get; set; }
        
        public decimal Balance { get; set; }
    }
}