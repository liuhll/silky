namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class DeductBalanceInput
    {
        public long AccountId { get; set; }

        public long OrderId { get; set; }

        public decimal OrderBalance { get; set; }
    }
}