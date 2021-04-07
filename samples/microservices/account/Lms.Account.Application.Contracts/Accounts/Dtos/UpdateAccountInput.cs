

using Silky.Lms.Rpc.Transport.CachingIntercept;

namespace Lms.Account.Application.Contracts.Accounts.Dtos
{
    public class UpdateAccountInput : CreateAccountInput
    {
        [CacheKey(0)]
        public long Id { get; set; }
    }
}