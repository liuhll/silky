using Silky.Lms.Rpc.Transport.CachingIntercept;

namespace Lms.Account.Application.Contracts.Accounts.Dtos
{
    public class DeductBalanceInput
    {
        [CacheKey(0)] public long AccountId { get; set; }

        public long OrderId { get; set; }

        public decimal OrderBalance { get; set; }
    }
}