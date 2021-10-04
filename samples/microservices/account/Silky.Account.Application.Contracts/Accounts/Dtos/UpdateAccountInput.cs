using Silky.Rpc.CachingInterceptor;

namespace Silky.Account.Application.Contracts.Accounts.Dtos
{
    public class UpdateAccountInput : CreateAccountInput
    {
        [CacheKey(0)]
        public long Id { get; set; }
    }
}