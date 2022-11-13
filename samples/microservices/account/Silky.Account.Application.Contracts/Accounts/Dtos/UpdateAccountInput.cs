namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class UpdateAccountInput : CreateAccountInput
    {
        public long Id { get; set; }
    }
}